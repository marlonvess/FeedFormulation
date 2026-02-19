using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Solver;

/// <summary>
/// Represents a single execution of a solver for a specific formula version within a tenant context, tracking its
/// status, timing, and results.
/// </summary>
/// <remarks>A SolverRun encapsulates all relevant information about a solver's execution, including the input
/// request, the user who initiated the run, the solver used, and the outcome. It provides status updates as the run
/// progresses through different states such as queued, running, failed, infeasible, or succeeded. Diagnostic messages
/// and detailed result data are available for analysis and troubleshooting. This class is intended for use in scenarios
/// where tracking and auditing of solver executions is required, such as in multi-tenant feed formulation
/// systems.</remarks>
public sealed class SolverRun : TenantEntity
{
    public Guid FormulaVersionId { get; private set; }
    public SolverRunStatus RunStatus { get; private set; } = SolverRunStatus.Queued;

    public string SolverName { get; private set; } = "HiGHS";
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public Guid RequestedByUserId { get; private set; }

    public string RequestJson { get; private set; } = "{}"; // Para debug 
    public string? ResultJson { get; private set; } // Resultado completo
    public string? DiagnosticMessage { get; private set; } // Erros

    // easy access to results
 

    private readonly List<SolverRunLineResult> _lineResults = new();
    public IReadOnlyCollection<SolverRunLineResult> LineResults => _lineResults;

    private readonly List<SolverRunNutrientResult> _nutrientResults = new();
    public IReadOnlyCollection<SolverRunNutrientResult> NutrientResults => _nutrientResults;

    private SolverRun() { }


    /// <summary>
    /// Initializes a new instance of the 
    /// SolverRun class with the specified tenant identifier, formula version identifier,
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="formulaVersionId"></param>
    /// <param name="requestedByUserId"></param>
    /// <param name="requestJson"></param>
    /// <param name="solverName"></param>
    public SolverRun(Guid tenantId, Guid formulaVersionId, Guid requestedByUserId, string requestJson, string solverName = "HiGHS")
    {
        TenantId = tenantId;
        FormulaVersionId = formulaVersionId;
        RequestedByUserId = requestedByUserId;
        RequestJson = requestJson;
        SolverName = solverName;
    }


    public void MarkRunning() { RunStatus = SolverRunStatus.Running; StartedAt = DateTime.UtcNow; }

    public void MarkFailed(string message)
    {
        RunStatus = SolverRunStatus.Failed; FinishedAt = DateTime.UtcNow; DiagnosticMessage = message;
    }

    public void MarkInfeasible(string message)
    {
        RunStatus = SolverRunStatus.Infeasible; FinishedAt = DateTime.UtcNow; DiagnosticMessage = message;
    }

    public void MarkSucceeded(string resultJson)
    {
        RunStatus = SolverRunStatus.Succeeded; FinishedAt = DateTime.UtcNow; ResultJson = resultJson;
    }
}