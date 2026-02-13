using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class IngredientPrice : TenantEntity
{
    public Guid IngredientId { get; private set; }
    public Guid? SupplierId { get; private set; }

    public decimal PricePerKg { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public bool IsDefault { get; private set; }

    private IngredientPrice() { }

    public IngredientPrice(Guid tenantId, Guid ingredientId, decimal pricePerKg, Guid? supplierId = null, DateTime? validFrom = null, bool isDefault = false)
    {
        TenantId = tenantId;
        IngredientId = ingredientId;
        PricePerKg = pricePerKg;
        SupplierId = supplierId;
        ValidFrom = validFrom;
        IsDefault = isDefault;
    }
}