using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Formulation;
/// <summary>
/// Represents a specific version of a formula associated with a tenant, including its configuration, approval status,
/// and related metadata.
/// </summary>
/// <remarks>A formula version encapsulates the details required to define and manage a particular iteration of a
/// formula, such as its target batch size, optimization objective, and any associated constraints. Approval information
/// is tracked to indicate whether the version has been reviewed and authorized for use. Use the Approve method to mark
/// a version as approved by a specific user. Instances of this class are immutable except for approval-related
/// properties, which are updated upon approval.</remarks>
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

    /// <summary>
    /// Initializes a new instance of the FormulaVersion class with the specified tenant, formula, version number,
    /// target batch size, optional constraint set, and notes.
    /// </summary>
    /// <remarks>If notes are provided, any leading or trailing whitespace is automatically removed before
    /// assignment.</remarks>
    /// <param name="tenantId">The unique identifier of the tenant associated with this formula version.</param>
    /// <param name="formulaId">The unique identifier of the formula to which this version belongs.</param>
    /// <param name="versionNumber">The version number that identifies the iteration of the formula.</param>
    /// <param name="targetBatchSizeKg">The target batch size, in kilograms, for which the formula is intended.</param>
    /// <param name="constraintSetId">An optional unique identifier for the constraint set associated with this formula version, or null if no
    /// constraint set is specified.</param>
    /// <param name="notes">Optional notes providing additional context or information about the formula version. Leading and trailing
    /// whitespace will be trimmed.</param>
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