using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FeedFormulation.Domain.Dtos.Solver;

// --- O que enviamos para o Python ---

public record SolverProblemRequest(
    string TenantId,
    string FormulaVersionId,
    decimal TargetBatchSizeKg,
    string Objective, // "min_cost"
    List<SolverIngredientInput> Ingredients,
    List<SolverConstraintInput> Constraints,
    List<SolverNutrientProfileInput> NutrientProfiles
);

public record SolverIngredientInput(
    string IngredientId,
    string Code,
    decimal CostPerKg,
    decimal? MinInclusionPercent = null,
    decimal? MaxInclusionPercent = null,
    decimal? FixedInclusionPercent = null
);

public record SolverNutrientProfileInput(
    string IngredientId,
    string NutrientId,
    decimal Value
);

public record SolverConstraintInput(
    string Type, // "nutrient_min", "nutrient_max", etc.
    string? NutrientId = null,
    string? IngredientId = null,
    string? IngredientGroupId = null,
    List<string>? IngredientIds = null,
    decimal? MinValue = null,
    decimal? MaxValue = null
);

// --- O que recebemos do Python ---

public record SolverProblemResponse(
    string Status, // "succeeded", "infeasible", "failed"
    string SolverName,
    decimal TotalCost,
    List<SolverIngredientResult> IngredientResults,
    List<SolverNutrientResult> NutrientResults,
    string? DiagnosticMessage = null
);

public record SolverIngredientResult(
    string IngredientId,
    decimal InclusionPercent,
    decimal CostContribution
);

public record SolverNutrientResult(
    string NutrientId,
    decimal AchievedValue,
    decimal? MinRequired,
    decimal? MaxAllowed,
    bool IsBinding
);