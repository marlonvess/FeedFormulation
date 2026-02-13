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