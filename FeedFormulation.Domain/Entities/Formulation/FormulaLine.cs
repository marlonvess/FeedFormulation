using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.ValueObjects;

namespace FeedFormulation.Domain.Entities.Formulation;

/// <summary>
/// Represents a line item in a formula, associating a specific ingredient with its inclusion percentages for a
/// particular formula version.
/// </summary>
/// <remarks>A FormulaLine defines the minimum, maximum, and fixed inclusion percentages for an ingredient within
/// a formula version. The inclusion percentages are represented by the Percentage value object, which enforces valid
/// percentage values. The IsActive property indicates whether the formula line is currently active and should be
/// considered in calculations or processing. This class is intended for use in feed formulation scenarios where precise
/// control over ingredient inclusion is required.</remarks>
public sealed class FormulaLine : TenantEntity
{
    public Guid FormulaVersionId { get; private set; }
    public Guid IngredientId { get; private set; }

    // Usando nosso ValueObject Percentage
    public Percentage? InclusionMinPercent { get; private set; }
    public Percentage? InclusionMaxPercent { get; private set; }
    public Percentage? FixedInclusionPercent { get; private set; }

    public bool IsActive { get; private set; } = true;

    private FormulaLine() { }

    public FormulaLine(Guid tenantId, Guid formulaVersionId, Guid ingredientId,
        Percentage? min, Percentage? max, Percentage? fixedValue)
    {
        TenantId = tenantId;
        FormulaVersionId = formulaVersionId;
        IngredientId = ingredientId;
        InclusionMinPercent = min;
        InclusionMaxPercent = max;
        FixedInclusionPercent = fixedValue;
    }
}