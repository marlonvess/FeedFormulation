using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Formulation;

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
}
