using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Catalog;
/// <summary>
/// Represents a nutrient within a tenant's catalog, including its identifying code, name, measurement unit, and active status.
/// </summary>
public sealed class Nutrient : TenantEntity
{
    /// <summary>
    /// Gets the code that uniquely identifies the entity.
    /// </summary>
    /// <remarks>The code is typically a short, immutable identifier such as "PB" or "EM". It is set during
    /// initialization and cannot be changed afterward.</remarks>
    public string Code { get; private set; } = null!; // Ex: "PB", "EM"
    public string Name { get; private set; } = null!;
    public NutrientUnit Unit { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Prevents direct instantiation of the Nutrient class outside of its containing type.
    /// </summary>
    /// <remarks>This private constructor is typically used to restrict object creation to factory methods or
    /// for use by serialization frameworks.</remarks>
    private Nutrient() { }

    /// <summary>
    /// Initializes a new instance of the Nutrient class with the specified tenant identifier, nutrient code, name, and
    /// unit of measurement.
    /// </summary>
    /// <remarks>The code and name parameters are normalized to ensure consistent formatting. This constructor
    /// is typically used when creating a new nutrient entity for a specific tenant context.</remarks>
    /// <param name="tenantId">The unique identifier of the tenant associated with this nutrient.</param>
    /// <param name="code">The code representing the nutrient. Leading and trailing whitespace are removed, and the code is converted to
    /// uppercase.</param>
    /// <param name="name">The name of the nutrient. Leading and trailing whitespace are removed.</param>
    /// <param name="unit">The unit of measurement for the nutrient, specified as a value of the NutrientUnit enumeration.</param>
    public Nutrient(Guid tenantId, string code, string name, NutrientUnit unit)
    {
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Unit = unit;
    }
}
