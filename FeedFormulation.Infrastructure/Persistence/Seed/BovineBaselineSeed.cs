using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Catalog;
using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Infrastructure.Persistence.Seed;

public static class BovineBaselineSeed
{
    public static async Task SeedAsync(AppDbContext db, Guid tenantId)
    {
        // 1. Ensure Basic Nutrients
        await EnsureNutrientsAsync(db, tenantId);

        // 2. Ensure Ingredient Groups
        await EnsureIngredientGroupsAsync(db, tenantId);
    }

    private static async Task EnsureNutrientsAsync(AppDbContext db, Guid tenantId)
    {
        // List of what we need to have in the database
        var required = new (string Code, string Name, NutrientUnit Unit)[]
        {
            ("PB",    "Proteína Bruta", NutrientUnit.Percent),
            ("NDF",   "Fibra Detergente Neutro", NutrientUnit.Percent),
            ("CA",    "Cálcio", NutrientUnit.Percent),
            ("P",     "Fósforo", NutrientUnit.Percent),
            ("NA",    "Sódio", NutrientUnit.Percent),
            ("EE",    "Extrato Etéreo (Gordura)", NutrientUnit.Percent),
            ("AMIDO", "Amido", NutrientUnit.Percent),
            ("MS",    "Matéria Seca", NutrientUnit.Percent)
        };

        // Verify which of the required nutrients are already in the database
        var existingCodes = await db.Nutrients
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Code)
            .ToListAsync();

        foreach (var (code, name, unit) in required)
        {
            if (existingCodes.Contains(code)) continue;

            db.Nutrients.Add(new Nutrient(tenantId, code, name, unit));
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Ensure the existence of the required ingredient groups for bovine feed formulation.
    /// </summary>
    /// <param name="db">The database context to use for querying and saving ingredient groups.</param>
    /// <param name="tenantId">The tenant ID to associate with the ingredient groups.</param>
    /// <returns>A task that represents the asynchronous operation of ensuring ingredient groups.</returns>
    private static async Task EnsureIngredientGroupsAsync(AppDbContext db, Guid tenantId)
    {
        var requiredGroups = new[]
        {
            "Volumosos",
            "Cereais/Amiláceos",
            "Mineral/Vitaminas",
            "Gorduras adicionadas",
            "Proteicos"
        };

        var existingNames = await db.IngredientGroups
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Name)
            .ToListAsync();

        foreach (var name in requiredGroups)
        {
            if (existingNames.Contains(name)) continue;

            db.IngredientGroups.Add(new IngredientGroup(tenantId, name));
        }

        await db.SaveChangesAsync();
    }
}