using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Common;
/// <summary>
/// Represents an entity that provides auditing information, including timestamps and user identifiers for creation and
/// updates.
/// </summary>
/// <remarks>Inherit from this abstract class to add auditing capabilities to domain entities. The auditing
/// properties are protected to ensure that only derived types can modify them, preserving the integrity of audit data.
/// This class is intended for scenarios where tracking the history of changes and responsible users is
/// required.</remarks>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }
    public Guid? UpdatedByUserId { get; protected set; }
}