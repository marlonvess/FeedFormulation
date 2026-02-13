using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Solver;

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

    // Relações para análise fácil
    private readonly List<SolverRunLineResult> _lineResults = new();
    public IReadOnlyCollection<SolverRunLineResult> LineResults => _lineResults;

    private readonly List<SolverRunNutrientResult> _nutrientResults = new();
    public IReadOnlyCollection<SolverRunNutrientResult> NutrientResults => _nutrientResults;

    private SolverRun() { }

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