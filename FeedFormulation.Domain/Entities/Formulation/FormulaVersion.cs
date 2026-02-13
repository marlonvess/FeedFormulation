using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Formulation;

public sealed class FormulaVersion : TenantEntity
{
    public Guid FormulaId { get; private set; }
    public int VersionNumber { get; private set; }

    public decimal TargetBatchSizeKg { get; private set; } = 1000m;
    public OptimizationObjective Objective { get; private set; } = OptimizationObjective.MinCost;
    public Guid? ConstraintSetId { get; private set; }

    public bool IsApproved { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<FormulaLine> _lines = new();
    public IReadOnlyCollection<FormulaLine> Lines => _lines;

    private FormulaVersion() { }

    public FormulaVersion(Guid tenantId, Guid formulaId, int versionNumber,
        decimal targetBatchSizeKg, Guid? constraintSetId, string? notes = null)
    {
        TenantId = tenantId;
        FormulaId = formulaId;
        VersionNumber = versionNumber;
        TargetBatchSizeKg = targetBatchSizeKg;
        ConstraintSetId = constraintSetId;
        Notes = notes?.Trim();
    }

    public void Approve(Guid approvedByUserId)
    {
        if (IsApproved) return;
        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
    }
}