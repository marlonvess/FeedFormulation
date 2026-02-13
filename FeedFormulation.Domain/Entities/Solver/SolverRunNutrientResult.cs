using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;


namespace FeedFormulation.Domain.Entities.Solver;

public sealed class SolverRunNutrientResult : TenantEntity
{
    public Guid SolverRunId { get; private set; }
    public Guid NutrientId { get; private set; }

    public decimal AchievedValue { get; private set; }
    public decimal? MinRequired { get; private set; }
    public decimal? MaxAllowed { get; private set; }
    public bool IsBinding { get; private set; } // Se a restrição limitou o resultado

    private SolverRunNutrientResult() { }

    public SolverRunNutrientResult(Guid tenantId, Guid solverRunId, Guid nutrientId, decimal achieved, decimal? minReq, decimal? maxAll, bool isBinding)
    {
        TenantId = tenantId;
        SolverRunId = solverRunId;
        NutrientId = nutrientId;
        AchievedValue = achieved;
        MinRequired = minReq;
        MaxAllowed = maxAll;
        IsBinding = isBinding;
    }
}