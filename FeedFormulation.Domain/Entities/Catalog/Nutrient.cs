using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class Nutrient : TenantEntity
{
    public string Code { get; private set; } = null!; // Ex: "PB", "EM"
    public string Name { get; private set; } = null!;
    public NutrientUnit Unit { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Nutrient() { }

    public Nutrient(Guid tenantId, string code, string name, NutrientUnit unit)
    {
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Unit = unit;
    }
}
