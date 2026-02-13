using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class Ingredient : TenantEntity
{
    public string Code { get; private set; } = null!; // Ex: "MILHO_01"
    public string Name { get; private set; } = null!; // Ex: "Milho Grão Moído"

    public decimal PriceCurrent { get; set; }
    public ICollection<IngredientNutrientProfile> NutritionalInfo { get; set; } = new List<IngredientNutrientProfile>();
    public IngredientCategory Category { get; private set; }
    public decimal? DryMatterPercent { get; private set; } // Matéria Seca (0..100)
    public bool IsActive { get; private set; } = true;

    // Construtor vazio para o EF Core
    private Ingredient() { }

    public Ingredient(Guid tenantId, string code, string name, IngredientCategory category)
    {
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Category = category;
    }
}
