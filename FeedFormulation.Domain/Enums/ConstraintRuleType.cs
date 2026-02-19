using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

/// <summary>
/// 
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
