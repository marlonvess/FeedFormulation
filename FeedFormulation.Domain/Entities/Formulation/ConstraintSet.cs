using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Formulation;
/// <summary>
/// Represents a set of constraint rules that apply to specific species and phases within a tenant context.
/// </summary>
/// <remarks>A ConstraintSet groups together multiple constraint rules that define the conditions under which
/// certain operations are valid for a given tenant. The set can be configured to target particular species and
/// production phases, allowing for flexible application of business logic. The IsActive property indicates whether the
/// constraint set is currently enforced. Use the Rules collection to access the individual constraint rules associated
/// with this set.</remarks>
public sealed class ConstraintSet : TenantEntity
{
    public string Name { get; private set; } = null!;
    public string? AppliesToSpecies { get; private set; } // Ex: "Bovinos"
    public string? AppliesToPhase { get; private set; }   // Ex: "Engorda"
    public bool IsActive { get; private set; } = true;

    private readonly List<ConstraintRule> _rules = new();
    public IReadOnlyCollection<ConstraintRule> Rules => _rules;

    private ConstraintSet() { }


    public ConstraintSet(Guid tenantId, string name, string? species = null, string? phase = null)
    {
        TenantId = tenantId;
        Name = name.Trim();
        AppliesToSpecies = species?.Trim();
        AppliesToPhase = phase?.Trim();
    }

    public void UpdateDetails(string name, string? species, string? phase)
    {
        Name = name.Trim();
        AppliesToSpecies = species?.Trim();
        AppliesToPhase = phase?.Trim();
    }
}
