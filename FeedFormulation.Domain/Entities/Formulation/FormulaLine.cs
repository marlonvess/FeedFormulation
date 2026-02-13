using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.ValueObjects;

namespace FeedFormulation.Domain.Entities.Formulation;

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