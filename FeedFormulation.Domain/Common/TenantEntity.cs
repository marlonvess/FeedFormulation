using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FeedFormulation.Domain.Common;

/// <summary>
/// Represents a base entity that includes a TenantId property for multi-tenancy support, along with auditing properties inherited from AuditableEntity.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public Guid TenantId { get; protected set; }
}