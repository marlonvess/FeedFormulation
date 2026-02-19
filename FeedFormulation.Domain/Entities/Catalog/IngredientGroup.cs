using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

/// <summary>
/// Represents a group of ingredients associated with a specific tenant.
/// </summary>
/// <remarks>An ingredient group is used to organize related ingredients within the context of a tenant,
/// supporting multi-tenancy scenarios. The group is identified by its name and provides access to its members through
/// the read-only Members collection. This class is typically used to manage and query ingredient groupings in
/// applications that require tenant isolation.</remarks>
public sealed class IngredientGroup : TenantEntity
{
    public string Name { get; private set; } = null!;

    // Relação de navegação (para o EF Core saber quem está no grupo)
    private readonly List<IngredientGroupMember> _members = new();
    public IReadOnlyCollection<IngredientGroupMember> Members => _members;

    private IngredientGroup() { }
    /// <summary>
    /// Initializes a new instance of the IngredientGroup class with the specified tenant identifier and group name.
    /// </summary>
    /// <remarks>The name parameter must not be null or empty. An exception may be thrown if this condition is
    /// not met.</remarks>
    /// <param name="tenantId">The unique identifier of the tenant associated with this ingredient group.</param>
    /// <param name="name">The name of the ingredient group. Leading and trailing whitespace is removed.</param>
    public IngredientGroup(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name.Trim();
    }
}