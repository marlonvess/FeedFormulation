using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Formulation;

/// <summary>
/// Represents a formula associated with a tenant, encapsulating its properties and versions.
/// </summary>
/// <remarks>A formula includes a name, an optional description, and may reference a default constraint set. The
/// status of a formula can be updated to archived using the Archive method. All versions of the formula are accessible
/// through the Versions property.</remarks>
public sealed class Formula : TenantEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public FormulaStatus Status { get; private set; } = FormulaStatus.Draft;
    public Guid? DefaultConstraintSetId { get; private set; }

    private readonly List<FormulaVersion> _versions = new();
    public IReadOnlyCollection<FormulaVersion> Versions => _versions;

    private Formula() { }

    public Formula(Guid tenantId, string name, string? description = null, Guid? defaultConstraintSetId = null)
    {
        TenantId = tenantId;
        Name = name.Trim();
        Description = description?.Trim();
        DefaultConstraintSetId = defaultConstraintSetId;
    }

    public void Archive() => Status = FormulaStatus.Archived;
}
