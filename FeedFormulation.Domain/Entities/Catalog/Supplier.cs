using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Catalog;

public sealed class Supplier : TenantEntity
{
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private Supplier() { }

    public Supplier(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name.Trim();
    }
}