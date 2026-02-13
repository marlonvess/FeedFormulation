using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Catalog;
using FeedFormulation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Infrastructure.Persistence.Seed;

public static class BovineIngredientSeed
{
    public static async Task SeedAsync(AppDbContext db, Guid tenantId)
    {
        // 1. Buscar Dicionários (Mapas) para não errar IDs
        // Precisamos saber qual é o ID do "PB" ou do grupo "Volumosos" no banco
        var nutrients = await db.Nutrients
            .Where(n => n.TenantId == tenantId)
            .ToDictionaryAsync(n => n.Code, n => n.Id);

        var groups = await db.IngredientGroups
            .Where(g => g.TenantId == tenantId)
            .ToDictionaryAsync(g => g.Name, g => g.Id);

        // Funções auxiliares para ficar mais limpo ler o código abaixo
        Guid N(string code) => nutrients.ContainsKey(code) ? nutrients[code] : Guid.Empty;
        Guid G(string name) => groups.ContainsKey(name) ? groups[name] : Guid.Empty;

        // 2. Definir a Lista de Ingredientes Iniciais
        var ingredientsData = new[]
        {
            new {
                Code="SIL_MILHO", Name="Silagem de Milho", Category=IngredientCategory.Cereal,
                Group="Volumosos", Price=0.07m,
                Nutrients=new Dictionary<string,decimal>{ {"PB",8.5m}, {"NDF",45m}, {"CA",0.25m}, {"P",0.22m}, {"NA",0.02m}, {"EE",3.0m}, {"AMIDO",30m} }
            },
            new {
                Code="FENO", Name="Feno de Erva", Category=IngredientCategory.Cereal,
                Group="Volumosos", Price=0.12m,
                Nutrients=new Dictionary<string,decimal>{ {"PB",12m}, {"NDF",55m}, {"CA",0.40m}, {"P",0.25m}, {"NA",0.03m}, {"EE",2.0m}, {"AMIDO",5m} }
            },
            new {
                Code="MILHO", Name="Milho Grão", Category=IngredientCategory.Cereal,
                Group="Cereais/Amiláceos", Price=0.25m,
                Nutrients=new Dictionary<string,decimal>{ {"PB",9.0m}, {"NDF",12m}, {"CA",0.02m}, {"P",0.30m}, {"NA",0.01m}, {"EE",4.0m}, {"AMIDO",65m} }
            },
            new {
                Code="SOJA48", Name="Farelo de Soja 48%", Category=IngredientCategory.Protein,
                Group="Proteicos", Price=0.52m,
                Nutrients=new Dictionary<string,decimal>{ {"PB",48m}, {"NDF",10m}, {"CA",0.30m}, {"P",0.65m}, {"NA",0.02m}, {"EE",1.5m}, {"AMIDO",2m} }
            },
            new {
                Code="MINERAL", Name="Corrector Mineral Bovinos", Category=IngredientCategory.Mineral,
                Group="Mineral/Vitaminas", Price=0.80m,
                Nutrients=new Dictionary<string,decimal>{ {"PB",0m}, {"NDF",0m}, {"CA",15m}, {"P",8m}, {"NA",5m}, {"EE",0m}, {"AMIDO",0m} }
            }
        };

        // 3. Inserir no Banco
        foreach (var data in ingredientsData)
        {
            // Verifica se já existe pelo Código
            var existing = await db.Ingredients
                .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Code == data.Code);

            if (existing != null) continue;

            // Criar Ingrediente
            var ingredient = new Ingredient(tenantId, data.Code, data.Name, data.Category);
            db.Ingredients.Add(ingredient);
            await db.SaveChangesAsync(); // Salva para gerar o ID

            // Associar ao Grupo
            if (G(data.Group) != Guid.Empty)
            {
                db.IngredientGroupMembers.Add(new IngredientGroupMember(tenantId, G(data.Group), ingredient.Id));
            }

            // Criar Preço Default
            db.IngredientPrices.Add(new IngredientPrice(tenantId, ingredient.Id, data.Price, isDefault: true, validFrom: DateTime.UtcNow));

            // Criar Perfil Nutricional (Matriz)
            foreach (var kv in data.Nutrients)
            {
                if (N(kv.Key) != Guid.Empty)
                {
                    db.IngredientNutrientProfiles.Add(new IngredientNutrientProfile(
                        tenantId, ingredient.Id, N(kv.Key), kv.Value, DataSourceType.Table, isDefault: true));
                }
            }

            await db.SaveChangesAsync();
        }
    }
}