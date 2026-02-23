using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;


namespace FeedFormulation.Domain.Entities.Solver;

public sealed class SolverRunNutrientResult : TenantEntity
{
    /// <summary>
    /// Solverrun identificator to which this nutrient result belongs. A single solver run will have multiple nutrient results, one for each nutrient constraint defined in the formula version.
    /// This allows us to track how well the solution achieved by the solver meets the nutritional requirements specified in the formula version, and to identify any constraints that may be binding or not met.
    /// </summary>
    public Guid SolverRunId { get; private set; }
    public Guid NutrientId { get; private set; }

    public decimal AchievedValue { get; private set; }
    public decimal? MinRequired { get; private set; }
    public decimal? MaxAllowed { get; private set; }
    public bool IsBinding { get; private set; } // Se a restrição limitou o resultado

    private SolverRunNutrientResult() { }

    
    /// <summary>
    ///
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="solverRunId"></param>
    /// <param name="nutrientId"></param>
    /// <param name="achieved"></param>
    /// <param name="minReq"></param>
    /// <param name="maxAll"></param>
    /// <param name="isBinding"></param>
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