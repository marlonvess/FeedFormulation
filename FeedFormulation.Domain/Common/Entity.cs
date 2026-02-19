using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Common;
/// <summary>
/// Represents a base class for entities that are uniquely identified by a GUID.
/// </summary>
/// <remarks>The <see cref="Id"/> property is initialized with a new globally unique identifier when an instance
/// is created. Derived classes should use this identifier to ensure uniqueness across entity instances.</remarks>


public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
