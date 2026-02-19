using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FeedFormulation.Domain.Common;

/// <summary>
/// 
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public Guid TenantId { get; protected set; }
}