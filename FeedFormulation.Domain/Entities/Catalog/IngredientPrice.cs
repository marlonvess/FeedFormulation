using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

/// <summary>
/// Represents the price information for a specific ingredient within a tenant, including supplier details, price per
/// kilogram, and validity period.
/// </summary>
/// <remarks>Use this class to manage and track ingredient pricing in multi-tenant scenarios. The price can be
/// associated with a particular supplier and may have an optional validity start date. The IsDefault property indicates
/// whether this price is the default for the ingredient within the tenant context.</remarks>
public sealed class IngredientPrice : TenantEntity
{
    /// <summary>
    /// Gets the unique identifier for the ingredient.
    /// </summary>
    public Guid IngredientId { get; private set; }
    public Guid? SupplierId { get; private set; }

    public decimal PricePerKg { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Prevents direct instantiation of the IngredientPrice class outside of its containing type.
    /// </summary>
    /// <remarks>This private constructor is typically used to restrict object creation, often for use with
    /// object-relational mappers or serialization frameworks that require a parameterless constructor.</remarks>
    private IngredientPrice() { }

    /// <summary>
    /// Initializes a new instance of the IngredientPrice class with the specified tenant, ingredient, price, and
    /// optional supplier, validity date, and default status.
    /// </summary>
    /// <remarks>Use this constructor to create an IngredientPrice instance when you need to specify supplier
    /// information, a validity start date, or mark the price as the default for the ingredient.</remarks>
    /// <param name="tenantId">The unique identifier of the tenant associated with the ingredient price.</param>
    /// <param name="ingredientId">The unique identifier of the ingredient for which the price is being set.</param>
    /// <param name="pricePerKg">The price per kilogram of the ingredient. Must be a non-negative decimal value.</param>
    /// <param name="supplierId">An optional unique identifier of the supplier providing the ingredient. If null, the supplier is unspecified.</param>
    /// <param name="validFrom">An optional date and time indicating when the price becomes valid. If null, the price is considered valid
    /// immediately.</param>
    /// <param name="isDefault">A value indicating whether this price is the default for the specified ingredient. The default is <see
    /// langword="false"/>.</param>
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