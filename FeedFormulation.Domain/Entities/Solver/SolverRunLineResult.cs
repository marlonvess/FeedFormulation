using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Solver;

/// <summary>
///
/// </summary>
public sealed class SolverRunLineResult : TenantEntity
{
    public Guid SolverRunId { get; private set; }
    public Guid IngredientId { get; private set; }
    public decimal InclusionPercent { get; private set; }
    public decimal CostContribution { get; private set; }

    private SolverRunLineResult() { }

    public SolverRunLineResult(Guid tenantId, Guid solverRunId, Guid ingredientId, decimal inclusionPercent, decimal costContribution)
    {
        TenantId = tenantId;
        SolverRunId = solverRunId;
        IngredientId = ingredientId;
        InclusionPercent = inclusionPercent;
        CostContribution = costContribution;
    }
}