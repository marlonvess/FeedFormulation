using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Dtos.Solver;
using FeedFormulation.Infrastructure.Persistence; 
using FeedFormulation.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;     
using FeedFormulation.Domain.Entities;           // To access Ingredient and Nutrient entities
using System.Text.Json;

namespace FeedFormulation.Application.Services;

public class FormulaService
{
    /// <summary>
    /// Gets the database context used for accessing the application data.
    /// </summary>
    /// <remarks>This field is initialized during the construction of the class and is used to interact with
    /// the underlying database. It is important to ensure that the context is properly disposed of to avoid memory
    /// leaks.</remarks>
    private readonly AppDbContext _context;
    private readonly SolverHttpClient _solverClient;

    public FormulaService(AppDbContext context, SolverHttpClient solverClient)
    {
        _context = context;
        _solverClient = solverClient;
    }

    /// <summary>
    /// Creates a sample bovine formula by retrieving ingredients from the database, mapping them to the solver format,
    /// and submitting a request to the solver service.
    /// </summary>
    /// <remarks>This method retrieves ingredients and their nutritional information from the database,
    /// constructs a solver request with constraints, and interacts with an external solver service to obtain a
    /// formulation result. It handles the response and updates the database with the solver run status.</remarks>
    /// <returns>A task that represents the asynchronous operation, containing the response from the solver indicating the status
    /// of the formula creation.</returns>
    /// <exception cref="Exception">Thrown if the database is empty, indicating that the seeds must be run first to populate the ingredients.</exception>
    public async Task<SolverProblemResponse> CreateSampleBovineFormulaAsync()
    {
        // 1. Search real ingredients on data base
        /// <remarks>Retrieves a list of ingredients from the database, including their associated nutritional information.
        /// If no ingredients are found, an exception is thrown to indicate that the database is empty and seeds should be run.</remarks>
        var ingredients = await _context.Ingredients
            .Include(i => i.NutritionalInfo)
            .ToListAsync();

        if (!ingredients.Any())
            throw new Exception("O banco de dados está vazio! Rode os Seeds primeiro.");

        // 2. Map to Solver DTOs
        /// <remarks>Maps the retrieved ingredients and their nutritional information to the format required by the solver service.
        var solverIngredients = ingredients.Select(i => new SolverIngredientInput(
            i.Id.ToString(),
            i.Name,
            i.PriceCurrent,
            0,   // Min
            100  // Max
        )).ToList();

        /// <remarks>Iterates through each ingredient and its nutritional information to create a list of nutrient profiles for the solver.
        /// Each profile includes the ingredient ID, nutrient ID, and the corresponding value.</remarks>
        var solverNutrients = new List<SolverNutrientProfileInput>();
        foreach (var ing in ingredients)
        {
            foreach (var nut in ing.NutritionalInfo)
            {
                solverNutrients.Add(new SolverNutrientProfileInput(
                    ing.Id.ToString(),
                    nut.NutrientId.ToString(), // Assuming we have the nutrient ID
                    nut.Value
                ));
            }
        }

        // 3.  Create an example constraint (e.g., Minimum 14% of Crude Protein)
        // Note: We need to know the exact ID of Crude Protein in your database.
       // here we will search for the nutrient with code "PB" (Proteína Bruta) to get its ID for the constraint.
        var pbNutrient = await _context.Nutrients.FirstOrDefaultAsync(n => n.Code == "PB");
        var constraints = new List<SolverConstraintInput>();

        if (pbNutrient != null)
        {
            constraints.Add(new SolverConstraintInput(
                "nutrient_min",
                pbNutrient.Id.ToString(),
                MinValue: 14.0m
            ));
        }

        // 4. Build the complete request
        /// <remarks>Constructs a complete solver problem request using the mapped ingredients, constraints, and nutrient profiles.
        var request = new SolverProblemRequest(
            TenantId: "11111111-1111-1111-1111-111111111111", // Our test ID
            FormulaVersionId: Guid.NewGuid().ToString(),
            TargetBatchSizeKg: 1000,
            Objective: "min_cost",
            Ingredients: solverIngredients,
            Constraints: constraints,
            NutrientProfiles: solverNutrients
        );


        //  Convert the request to JSON (to keep an exact record of what we sent)
        var requestJson = System.Text.Json.JsonSerializer.Serialize(request);

        // Create the record in the database (Still as Running/Queued)
        var solverRun = new FeedFormulation.Domain.Entities.Solver.SolverRun(
            tenantId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            formulaVersionId: Guid.Parse(request.FormulaVersionId),
            requestedByUserId: Guid.Empty, // using an anonymous user for now
            requestJson: requestJson
        );
        solverRun.MarkRunning();
        _context.SolverRuns.Add(solverRun);

        // 5. Call python solver and wait for the response
        var response = await _solverClient.SolveAsync(request);

        // Convert the response to JSON (to keep an exact record of what we received)
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);

        // 6. check the response and update the SolverRun record accordingly
        if (response.Status == "succeeded")
        {
            solverRun.MarkSucceeded(responseJson);
        }
        else if (response.Status == "infeasible")
        {
            solverRun.MarkInfeasible(response.DiagnosticMessage ?? "Fórmula impossível de resolver.");
        }
        else
        {
            solverRun.MarkFailed(response.DiagnosticMessage ?? "Erro no solver.");
        }

        // 7. save the changes to the database
        await _context.SaveChangesAsync();

        return response;
    }
}