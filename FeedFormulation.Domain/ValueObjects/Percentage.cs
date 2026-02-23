using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.ValueObjects;

/// <summary>
/// Represents a percentage value constrained between 0 and 100. This value object ensures that any percentage used within the domain is valid and provides implicit conversions to and from decimal for ease of use in calculations and comparisons.
/// </summary>
public readonly record struct Percentage
{
    
    // Declaramos a propriedade explicitamente aqui:
    public decimal Value { get; }

    // este construtor é o único e a validação funcionará
    public Percentage(decimal value)
    {
        if (value < 0m || value > 100m)
            throw new ArgumentOutOfRangeException(nameof(value), "Percentagem deve estar entre 0 e 100.");

        Value = value;
    }

    // Permite usar "decimal" e "Percentage" de forma intercambiável
    public static implicit operator decimal(Percentage p) => p.Value;
    public static implicit operator Percentage(decimal v) => new Percentage(v);
}