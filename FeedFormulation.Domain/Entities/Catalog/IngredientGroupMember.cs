using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

/// <summary>
/// Represents the association between a specific ingredient and an ingredient group within the context of a tenant.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It is used to establish a relationship between an
/// ingredient and its group for a particular tenant, supporting multi-tenant catalog scenarios.</remarks>
public sealed class IngredientGroupMember : TenantEntity
{
    public Guid IngredientGroupId { get; private set; }
    public Guid IngredientId { get; private set; }

    private IngredientGroupMember() { }
    /// <summary>
    /// Initializes a new instance of the IngredientGroupMember class using the specified tenant, group, and ingredient
    /// identifiers.
    /// </summary>
    /// <remarks>All identifiers are required to ensure the creation of a valid ingredient group member.
    /// Supplying an empty GUID for any parameter may result in an invalid state.</remarks>
    /// <param name="tenantId">The unique identifier of the tenant associated with this ingredient group member. This value must not be an
    /// empty GUID.</param>
    /// <param name="groupId">The unique identifier of the ingredient group to which this member belongs. This value must not be an empty
    /// GUID.</param>
    /// <param name="ingredientId">The unique identifier of the ingredient represented by this member. This value must not be an empty GUID.</param>
    public IngredientGroupMember(Guid tenantId, Guid groupId, Guid ingredientId)
    {
        TenantId = tenantId;
        IngredientGroupId = groupId;
        IngredientId = ingredientId;
    }
}