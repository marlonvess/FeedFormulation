using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Dtos.Solver;
using FeedFormulation.Infrastructure.Persistence; 
using FeedFormulation.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;     
using FeedFormulation.Domain.Entities;           // Para a entidade Ingredient e Nutrient
using System.Text.Json;

namespace FeedFormulation.Application.Services;

public class FormulaService
{
    private readonly AppDbContext _context;
    private readonly SolverHttpClient _solverClient;

    public FormulaService(AppDbContext context, SolverHttpClient solverClient)
    {
        _context = context;
        _solverClient = solverClient;
    }

    public async Task<SolverProblemResponse> CreateSampleBovineFormulaAsync()
    {
        // 1. Buscar ingredientes reais do Banco de Dados
        var ingredients = await _context.Ingredients
            .Include(i => i.NutritionalInfo)
            .ToListAsync();

        if (!ingredients.Any())
            throw new Exception("O banco de dados está vazio! Rode os Seeds primeiro.");

        // 2. Mapear para o formato do Solver (DTOs)
        var solverIngredients = ingredients.Select(i => new SolverIngredientInput(
            i.Id.ToString(),
            i.Name,
            i.PriceCurrent,
            0,   // Min
            100  // Max
        )).ToList();

        var solverNutrients = new List<SolverNutrientProfileInput>();
        foreach (var ing in ingredients)
        {
            foreach (var nut in ing.NutritionalInfo)
            {
                solverNutrients.Add(new SolverNutrientProfileInput(
                    ing.Id.ToString(),
                    nut.NutrientId.ToString(), // Assumindo que temos o ID do nutriente
                    nut.Value
                ));
            }
        }

        // 3. Criar uma restrição de exemplo (Ex: Mínimo 14% de Proteína Bruta)
        // Nota: Precisamos saber o ID exato da Proteína Bruta no seu banco. 
        // Aqui vou usar um ID fictício ou string. O ideal é buscar pelo Code.
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

        // 4. Montar o Pedido Completo
        var request = new SolverProblemRequest(
            TenantId: "11111111-1111-1111-1111-111111111111", // Nosso ID de teste
            FormulaVersionId: Guid.NewGuid().ToString(),
            TargetBatchSizeKg: 1000,
            Objective: "min_cost",
            Ingredients: solverIngredients,
            Constraints: constraints,
            NutrientProfiles: solverNutrients
        );


        // Converter o pedido para JSON (para guardar um registo exato do que enviamos)
        var requestJson = System.Text.Json.JsonSerializer.Serialize(request);

        // Criar o registo no banco de dados (Ainda como Running/Queued)
        var solverRun = new FeedFormulation.Domain.Entities.Solver.SolverRun(
            tenantId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            formulaVersionId: Guid.Parse(request.FormulaVersionId),
            requestedByUserId: Guid.Empty, // Utilizador anónimo por enquanto
            requestJson: requestJson
        );
        solverRun.MarkRunning();
        _context.SolverRuns.Add(solverRun);

        // 5. Chamar o Python!
        var response = await _solverClient.SolveAsync(request);

        // Converter a resposta para JSON
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);

        // 6. Atualizar o estado do registo consoante a resposta do Python
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

        // 7. Guardar tudo permanentemente no banco de dados!
        await _context.SaveChangesAsync();

        return response;
    }
}