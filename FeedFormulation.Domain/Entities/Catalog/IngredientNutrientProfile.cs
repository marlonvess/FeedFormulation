using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class IngredientNutrientProfile : TenantEntity
{
    public Guid IngredientId { get; private set; }
    public Guid NutrientId { get; private set; }

    public decimal Value { get; private set; } // O valor (ex: 8.5)
    public DataSourceType Source { get; private set; } // Ex: Tabela INRA ou Laboratório
    public bool IsDefault { get; private set; } // Se é o valor padrão usado no cálculo

    private IngredientNutrientProfile() { }

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