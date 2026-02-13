using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FeedFormulation.Infrastructure.Persistence.Configurations;

public sealed class FormulaLineConfiguration : IEntityTypeConfiguration<FormulaLine>
{
    public void Configure(EntityTypeBuilder<FormulaLine> b)
    {
        b.ToTable("formula_lines");
        b.HasKey(x => x.Id);

        // 1. Criamos um conversor explícito que ensina:
        //    IDA: Percentage -> decimal (pega o .Value)
        //    VOLTA: decimal -> Percentage (cria um new Percentage)
        var percentageConverter = new ValueConverter<Percentage, decimal>(
            v => v.Value,
            v => new Percentage(v));

        // 2. Aplicamos esse conversor nas propriedades
        // O EF Core é inteligente o suficiente para aplicar isso automaticamente
        // mesmo que a propriedade seja "Percentage?" (nula).

        b.Property(x => x.InclusionMinPercent)
         .HasConversion(percentageConverter)
         .HasColumnName("inclusion_min_percent")
         .HasColumnType("decimal(5,2)"); // Força o tipo SQL explicitamente

        b.Property(x => x.InclusionMaxPercent)
         .HasConversion(percentageConverter)
         .HasColumnName("inclusion_max_percent")
         .HasColumnType("decimal(5,2)");

        b.Property(x => x.FixedInclusionPercent)
         .HasConversion(percentageConverter)
         .HasColumnName("fixed_inclusion_percent")
         .HasColumnType("decimal(5,2)");
    }
}