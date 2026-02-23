using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

/// <summary>
/// Defines the various types of constraint rules that can be applied in feed formulation.
/// </summary>
public enum ConstraintRuleType
{
    NutrientMin,
    NutrientMax,
    IngredientMin,
    IngredientMax,
    GroupSumMin,   // Ex: Mínimo de Volumosos
    GroupSumMax    // Ex: Máximo de Cereais
}
