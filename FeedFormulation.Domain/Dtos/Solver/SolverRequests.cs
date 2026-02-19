using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FeedFormulation.Domain.Dtos.Solver;

/// <summary>
/// Represents a request to solve a feed formulation problem using specified ingredients, constraints, and nutrient
/// profiles. This type encapsulates all necessary information for optimizing a formula according to a given objective
/// and batch size.
/// </summary>
/// <remarks>This request is typically used as input to a feed formulation solver, which processes the provided
/// data to generate an optimized formula. Ensure that all lists are populated with valid entries and that the batch
/// size and objective are appropriate for the intended use case.</remarks>
/// <param name="TenantId">The unique identifier for the tenant making the request. Used to scope the problem to a specific organization or
/// user context.</param>
/// <param name="FormulaVersionId">The identifier for the version of the formula to be used in the solving process. Determines which formula
/// configuration is applied.</param>
/// <param name="TargetBatchSizeKg">The target batch size, in kilograms, for which the solution should be optimized. Must be a positive value.</param>
/// <param name="Objective">The objective of the optimization, such as 'min_cost' to minimize costs. Specifies the goal that the solver should
/// achieve.</param>
/// <param name="Ingredients">A list of ingredients to be considered in the optimization process. Each ingredient is defined by a
/// SolverIngredientInput and influences the solution space.</param>
/// <param name="Constraints">A list of constraints that must be satisfied in the solution. Each constraint is defined by a SolverConstraintInput
/// and restricts the feasible solutions.</param>
/// <param name="NutrientProfiles">A list of nutrient profiles that the solution must meet. Each profile is defined by a SolverNutrientProfileInput and
/// sets nutritional targets or limits.</param>
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

/// <summary>
/// Represents the input parameters for an ingredient used in a feed formulation solver, including identification, cost,
/// and inclusion constraints.
/// </summary>
/// <remarks>Use this record to define constraints and cost information for each ingredient when preparing input
/// for feed formulation optimization. Inclusion percentages are expressed as values between 0 and 100, representing the
/// proportion of the ingredient in the final mix.</remarks>
/// <param name="IngredientId">The unique identifier for the ingredient. Used to reference the ingredient within the solver.</param>
/// <param name="Code">The code associated with the ingredient. May be used for categorization or external identification.</param>
/// <param name="CostPerKg">The cost of the ingredient per kilogram. Influences the overall cost calculations in the solver.</param>
/// <param name="MinInclusionPercent">The minimum percentage of this ingredient that must be included in the formulation. Specify null if no minimum is
/// required.</param>
/// <param name="MaxInclusionPercent">The maximum percentage of this ingredient that can be included in the formulation. Specify null if no maximum is
/// required.</param>
/// <param name="FixedInclusionPercent">A fixed percentage of this ingredient that must be included in the formulation. Specify null if the inclusion is not
/// fixed.</param>
public record SolverIngredientInput(
    string IngredientId,
    string Code,
    decimal CostPerKg,
    decimal? MinInclusionPercent = null,
    decimal? MaxInclusionPercent = null,
    decimal? FixedInclusionPercent = null
);

/// <summary>
/// Represents the input parameters required to define a nutrient profile for a specific ingredient in a solver
/// operation.  
/// </summary>
/// <param name="IngredientId">The unique identifier of the ingredient for which the nutrient profile is being specified.</param>
/// <param name="NutrientId">The unique identifier of the nutrient to be evaluated or constrained within the profile.</param>
/// <param name="Value">The quantity of the specified nutrient associated with the given ingredient. Must be a non-negative decimal value.</param>
public record SolverNutrientProfileInput(
    string IngredientId,
    string NutrientId,
    decimal Value
);
/// <summary>
///     Represents a constraint applied to a solver, specifying limits or requirements for nutrients, ingredients, or
/// ingredient groups within a formulation scenario.
/// </summary>
/// <remarks>Use this record to define input constraints for solver operations, such as minimum or maximum
/// nutrient levels, ingredient inclusion limits, or group restrictions. Only relevant parameters need to be specified
/// for each constraint type; unused parameters can be left null.</remarks>
/// <param name="Type">The type of constraint to apply. Supported values include 'nutrient_min', 'nutrient_max', and other constraint types
/// as defined by the solver.</param>
/// <param name="NutrientId">The identifier of the nutrient to which the constraint applies, if applicable. Specify null if the constraint is not
/// nutrient-specific.</param>
/// <param name="IngredientId">The identifier of a specific ingredient affected by the constraint, if applicable. Specify null if the constraint
/// does not target a single ingredient.</param>
/// <param name="IngredientGroupId">The identifier of an ingredient group affected by the constraint, if applicable. Specify null if the constraint does
/// not target a group.</param>
/// <param name="IngredientIds">A list of ingredient identifiers encompassed by the constraint, if applicable. Specify null if the constraint does
/// not target multiple ingredients.</param>
/// <param name="MinValue">The minimum value for the constraint, if a lower bound is required. Specify null if no minimum is enforced.</param>
/// <param name="MaxValue">The maximum value for the constraint, if an upper bound is required. Specify null if no maximum is enforced.</param>
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

/// <summary>
/// Represents the response from a feed formulation solver after processing a given problem request. This record encapsulates
/// </summary>
/// <param name="Status"></param>
/// <param name="SolverName"></param>
/// <param name="TotalCost"></param>
/// <param name="IngredientResults"></param>
/// <param name="NutrientResults"></param>
/// <param name="DiagnosticMessage"></param>
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