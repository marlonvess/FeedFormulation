using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

/// <summary>
/// Defines the various statuses that a feed formulation can have throughout its lifecycle. This enumeration helps track the state of a formulation, from its initial creation to its final approval and potential archiving.
/// </summary>
public enum FormulaStatus
{
    Draft,      
    Approved,   
    Archived   
}