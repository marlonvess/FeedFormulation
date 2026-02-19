using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Formulation;


public sealed class ConstraintRule : TenantEntity
{
    /// <summary>
    /// Gets the unique identifier for the constraint set associated with this instance.
    /// </summary>
    /// <remarks>This identifier is used to reference a specific set of constraints within the application. It
    /// is generated upon creation and cannot be modified after initialization.</remarks>
    public Guid ConstraintSetId { get; private set; }
    public ConstraintRuleType Type { get; private set; } // MinNutrient, MaxIngredient, etc.

    // Pode ser regra de Nutriente, Ingrediente ou Grupo
    public Guid? NutrientId { get; private set; }
    public Guid? IngredientId { get; private set; }
    public Guid? IngredientGroupId { get; private set; }
  
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }

    public bool IsHardConstraint { get; private set; } = true;
    public string? Notes { get; private set; }

    private ConstraintRule() { }
    /// <summary>
    /// Initializes a new instance of the ConstraintRule class, representing a rule that constrains values based on
    /// specified criteria within a formulation context.
    /// </summary>
    /// <remarks>Use this constructor to define a constraint rule for a specific tenant and constraint set,
    /// optionally targeting a nutrient, ingredient, or ingredient group. At least one of nutrientId, ingredientId, or
    /// ingredientGroupId should typically be specified to indicate the target of the constraint.</remarks>
    /// <param name="tenantId">The unique identifier of the tenant to which this constraint rule applies.</param>
    /// <param name="constraintSetId">The unique identifier of the constraint set that contains this rule.</param>
    /// <param name="type">The type of constraint rule, which determines the nature of the constraint applied.</param>
    /// <param name="minValue">The minimum allowable value for the constraint. Specify null if no minimum is required.</param>
    /// <param name="maxValue">The maximum allowable value for the constraint. Specify null if no maximum is required.</param>
    /// <param name="nutrientId">An optional unique identifier for the nutrient to which this constraint applies. Specify null if the constraint
    /// is not nutrient-specific.</param>
    /// <param name="ingredientId">An optional unique identifier for the ingredient to which this constraint applies. Specify null if the
    /// constraint is not ingredient-specific.</param>
    /// <param name="ingredientGroupId">An optional unique identifier for the ingredient group to which this constraint applies. Specify null if the
    /// constraint is not group-specific.</param>
    /// <param name="isHard">true to enforce the constraint as a hard (mandatory) rule; otherwise, false to treat it as a soft (advisory)
    /// constraint.</param>
    /// <param name="notes">Optional notes or comments describing the constraint. Specify null if no notes are required.</param>
    public ConstraintRule(Guid tenantId, Guid constraintSetId, ConstraintRuleType type,
        decimal? minValue, decimal? maxValue,
        Guid? nutrientId = null, Guid? ingredientId = null, Guid? ingredientGroupId = null,
        bool isHard = true, string? notes = null)
    {
        TenantId = tenantId;
        ConstraintSetId = constraintSetId;
        Type = type;
        NutrientId = nutrientId;
        IngredientId = ingredientId;
        IngredientGroupId = ingredientGroupId;
        MinValue = minValue;
        MaxValue = maxValue;
        IsHardConstraint = isHard;
        Notes = notes?.Trim();
    }
}