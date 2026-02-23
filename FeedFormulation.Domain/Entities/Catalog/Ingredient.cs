using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Catalog;

/// <summary>
/// Represents an ingredient within a tenant's catalog, including its identifying code, name, current price, nutritional
/// information, and category.
/// </summary>
/// <remarks>Use this class to manage ingredient data in the context of a multi-tenant feed formulation system.
/// The ingredient is active by default and supports tracking of nutritional profiles and pricing. Properties such as
/// code and name are required and uniquely identify the ingredient within a tenant's scope.</remarks>
public sealed class Ingredient : TenantEntity
{
    public string Code { get; private set; } = null!; // Ex: "MILHO_01"
    public string Name { get; private set; } = null!; // Ex: "Milho Grão Moído"

    public decimal PriceCurrent { get; set; }
    public ICollection<IngredientNutrientProfile> NutritionalInfo { get; set; } = new List<IngredientNutrientProfile>();
    public IngredientCategory Category { get; private set; }
    public decimal? DryMatterPercent { get; private set; } // Matéria Seca (0..100)
    public bool IsActive { get; private set; } = true;

    // Empty constructor for EF Core
    /// <summary>
    /// Initializes a new instance of the Ingredient class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core during materialization and
    /// should not be called directly in application code.</remarks>
    private Ingredient() { }

    /// <summary>
    /// Initializes a new instance of the Ingredient class with the specified tenant ID, code, name, and category.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="code"></param>
    /// <param name="name"></param>
    /// <param name="category"></param>
    public Ingredient(Guid tenantId, string code, string name, IngredientCategory category)
    {
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Category = category;
    }
}
