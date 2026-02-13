using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Infrastructure.Persistence.Seed;

public static class BovineConstraintSetsSeed
{
    public static async Task SeedAsync(AppDbContext db, Guid tenantId)
    {
        // 1. Buscar IDs necessários
        var nutrients = await db.Nutrients
            .Where(n => n.TenantId == tenantId)
            .ToDictionaryAsync(n => n.Code, n => n.Id);

        var groups = await db.IngredientGroups
            .Where(g => g.TenantId == tenantId)
            .ToDictionaryAsync(g => g.Name, g => g.Id);

        Guid N(string code) => nutrients.ContainsKey(code) ? nutrients[code] : Guid.Empty;
        Guid G(string name) => groups.ContainsKey(name) ? groups[name] : Guid.Empty;

        // 2. Definir o Template de Engorda
        var engordaName = "Bovinos Engorda - Acabamento (TMR) - Base MS";
        var engorda = await db.ConstraintSets
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == engordaName);

        if (engorda is null)
        {
            // Cria o Cabeçalho do Conjunto de Regras
            engorda = new ConstraintSet(tenantId, engordaName, species: "Bovinos", phase: "Engorda/Acabamento");
            db.ConstraintSets.Add(engorda);
            await db.SaveChangesAsync();

            // Cria as Regras Individuais
            var rules = new List<ConstraintRule>
            {
                // Regras Nutricionais (Baseado em Matéria Seca)
                RuleNutrientMin(tenantId, engorda.Id, N("PB"), 11.0m),
                RuleNutrientMax(tenantId, engorda.Id, N("PB"), 15.0m),

                RuleNutrientMin(tenantId, engorda.Id, N("NDF"), 28.0m),
                RuleNutrientMax(tenantId, engorda.Id, N("NDF"), 40.0m),

                RuleNutrientMax(tenantId, engorda.Id, N("EE"), 6.0m), // Gordura
                RuleNutrientMax(tenantId, engorda.Id, N("AMIDO"), 35.0m),

                RuleNutrientMin(tenantId, engorda.Id, N("CA"), 0.45m),
                RuleNutrientMax(tenantId, engorda.Id, N("CA"), 0.90m),

                RuleNutrientMin(tenantId, engorda.Id, N("P"), 0.25m),
                RuleNutrientMin(tenantId, engorda.Id, N("NA"), 0.10m),

                // Regras de Grupos (Limites Tecnológicos/Saúde)
                RuleGroupMin(tenantId, engorda.Id, G("Volumosos"), 30.0m), // Mínimo 30% de volumoso para saúde do rúmen
                RuleGroupMax(tenantId, engorda.Id, G("Cereais/Amiláceos"), 55.0m),
                RuleGroupMin(tenantId, engorda.Id, G("Mineral/Vitaminas"), 1.0m),
                RuleGroupMax(tenantId, engorda.Id, G("Gorduras adicionadas"), 2.0m)
            };

            // Filtra regras inválidas (caso algum ID não tenha sido encontrado) e salva
            db.ConstraintRules.AddRange(rules.Where(r => r.NutrientId != Guid.Empty || r.IngredientGroupId != Guid.Empty));
            await db.SaveChangesAsync();
        }
    }

    // Métodos Auxiliares para criar as regras mais facilmente
    private static ConstraintRule RuleNutrientMin(Guid tenantId, Guid setId, Guid nutrientId, decimal min)
        => new(tenantId, setId, ConstraintRuleType.NutrientMin, minValue: min, maxValue: null, nutrientId: nutrientId);

    private static ConstraintRule RuleNutrientMax(Guid tenantId, Guid setId, Guid nutrientId, decimal max)
        => new(tenantId, setId, ConstraintRuleType.NutrientMax, minValue: null, maxValue: max, nutrientId: nutrientId);

    private static ConstraintRule RuleGroupMin(Guid tenantId, Guid setId, Guid groupId, decimal min)
        => new(tenantId, setId, ConstraintRuleType.GroupSumMin, minValue: min, maxValue: null, ingredientGroupId: groupId);

    private static ConstraintRule RuleGroupMax(Guid tenantId, Guid setId, Guid groupId, decimal max)
        => new(tenantId, setId, ConstraintRuleType.GroupSumMax, minValue: null, maxValue: max, ingredientGroupId: groupId);
}