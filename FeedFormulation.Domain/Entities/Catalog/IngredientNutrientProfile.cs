using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Catalog;

/// <summary>
/// Represents the nutrient profile of an ingredient, linking an ingredient to a specific nutrient and its value, source, and whether it's the default for calculations.
/// </summary>
public sealed class IngredientNutrientProfile : TenantEntity
{
    public Guid IngredientId { get; private set; }
    public Guid NutrientId { get; private set; }

    public decimal Value { get; private set; } // value of the nutrient in the ingredient E.G. 8.5% of crude protein
    public DataSourceType Source { get; private set; } // E.G. "INRA Table" or "Laboratory Analysis"
    public bool IsDefault { get; private set; } // If this vale is the default for calculations (e.g., if true, this value will be used in formulations unless a specific override is provided)

    private IngredientNutrientProfile() { }

    /// <summary>
    /// Creates a new instance of IngredientNutrientProfile.
    /// </summary>
    /// <param name="tenantId">The tenant ID to which this profile belongs.</param>
    /// <param name="ingredientId">The ID of the ingredient.</param>
    /// <param name="nutrientId">The ID of the nutrient.</param>
    /// <param name="value">The value of the nutrient in the ingredient (e.g., 8.5).</param>
    /// <param name="source">The source of the nutrient data (e.g., INRA Table or Laboratory).</param>
    /// <param name="isDefault">Indicates whether this value is the default used in calculations.</param>
    public IngredientNutrientProfile(Guid tenantId, Guid ingredientId, Guid nutrientId, decimal value, DataSourceType source, bool isDefault = false)
    {
        TenantId = tenantId;
  
        IngredientId = ingredientId;
        NutrientId = nutrientId;
        Value = value;
        Source = source;
        IsDefault = isDefault;
    }
}