using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class IngredientGroupMember : TenantEntity
{
    public Guid IngredientGroupId { get; private set; }
    public Guid IngredientId { get; private set; }

    private IngredientGroupMember() { }

    public IngredientGroupMember(Guid tenantId, Guid groupId, Guid ingredientId)
    {
        TenantId = tenantId;
        IngredientGroupId = groupId;
        IngredientId = ingredientId;
    }
}