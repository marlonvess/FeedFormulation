using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Livestock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedFormulation.Infrastructure.Persistence.Configurations;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        // Define o nome da tabela
        builder.ToTable("Animals");

        // Define a Chave Primária
        builder.HasKey(a => a.Id);

        // O SIA (Brinco Oficial) é obrigatório e único por cada Fazenda (Tenant)
        builder.Property(a => a.SiaNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => new { a.TenantId, a.SiaNumber })
            .IsUnique();

        // Limita o tamanho das strings para poupar espaço no banco
        builder.Property(a => a.Name).HasMaxLength(100);
        builder.Property(a => a.InternalNumber).HasMaxLength(20);
        builder.Property(a => a.Breed).HasMaxLength(50);
        builder.Property(a => a.Group).HasMaxLength(50);
        builder.Property(a => a.Lot).HasMaxLength(50);
        builder.Property(a => a.SireSia).HasMaxLength(20);
        builder.Property(a => a.DamSia).HasMaxLength(20);

        // Define os limites das propriedades numéricas
        builder.Property(a => a.LastMilkProduction)
            .HasPrecision(5, 2); // Ex: 123.45 litros
    }
}