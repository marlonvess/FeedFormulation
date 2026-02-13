using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class IngredientGroup : TenantEntity
{
    public string Name { get; private set; } = null!;

    // Relação de navegação (para o EF Core saber quem está no grupo)
    private readonly List<IngredientGroupMember> _members = new();
    public IReadOnlyCollection<IngredientGroupMember> Members => _members;

    private IngredientGroup() { }

    public IngredientGroup(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name.Trim();
    }
}