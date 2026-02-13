using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }
    public Guid? UpdatedByUserId { get; protected set; }
}