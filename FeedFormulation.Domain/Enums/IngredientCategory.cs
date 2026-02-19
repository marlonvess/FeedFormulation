using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

/// <summary>
/// Defines the various categories of
/// ingredients that can be used in feed formulation. This enumeration helps classify
/// </summary>
public enum IngredientCategory
{
    Cereal,
    Protein,
    Fat,
    Mineral,
    Additive,
    ByProduct,
    Other
}
