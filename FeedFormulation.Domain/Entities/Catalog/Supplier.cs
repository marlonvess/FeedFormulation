using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

/// <summary>
/// Represents a supplier associated with a specific tenant, including the supplier's name and active status.
/// </summary>
/// <remarks>The supplier's name is trimmed of leading and trailing whitespace when the instance is created. New
/// suppliers are initialized as active by default.</remarks>
public sealed class Supplier : TenantEntity
{
    /// <summary>
    /// Gets the name associated with the current instance.
    /// </summary>
    /// <remarks>The name is initialized to a non-null value and can only be set internally within the
    /// class.</remarks>
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 
    /// </summary>
    private Supplier() { }

/// <summary>
/// 
/// </summary>
/// <param name="tenantId"></param>
/// <param name="name"></param>
    public Supplier(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name.Trim();
    }
}