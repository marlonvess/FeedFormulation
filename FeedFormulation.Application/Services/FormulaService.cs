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
            TenantId: "demo-tenant",
            FormulaVersionId: Guid.NewGuid().ToString(),
            TargetBatchSizeKg: 1000,
            Objective: "min_cost",
            Ingredients: solverIngredients,
            Constraints: constraints,
            NutrientProfiles: solverNutrients
        );

        // 5. Chamar o Python!
        return await _solverClient.SolveAsync(request);
    }
}