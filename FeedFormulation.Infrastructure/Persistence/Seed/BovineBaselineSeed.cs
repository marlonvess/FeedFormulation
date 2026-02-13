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
        // 1. Garantir Nutrientes Básicos
        await EnsureNutrientsAsync(db, tenantId);

        // 2. Garantir Grupos de Ingredientes
        await EnsureIngredientGroupsAsync(db, tenantId);
    }

    private static async Task EnsureNutrientsAsync(AppDbContext db, Guid tenantId)
    {
        // Lista do que precisamos ter no banco
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

        // Verifica o que JÁ existe para não duplicar
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