using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Livestock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedFormulation.Infrastructure.Persistence.Configurations;

public class MilkProductionRecordConfiguration : IEntityTypeConfiguration<MilkProductionRecord>
{
    public void Configure(EntityTypeBuilder<MilkProductionRecord> builder)
    {
        // Nome da tabela
        builder.ToTable("MilkProductionRecords");

        // Chave Primária
        builder.HasKey(m => m.Id);

        // Precisão dos números decimais (Tamanho total, Casas decimais)
        builder.Property(m => m.VolumeInLiters).HasPrecision(5, 2); // Ex: 123.45 Litros
        builder.Property(m => m.FatPercentage).HasPrecision(4, 2);  // Ex: 3.50 %
        builder.Property(m => m.ProteinPercentage).HasPrecision(4, 2); // Ex: 3.20 %

        // Configuração da Relação (A Ponte entre as tabelas)
        builder.HasOne(m => m.Animal)
            .WithMany(a => a.MilkProductions)
            .HasForeignKey(m => m.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
        // Cascade: Se a vaca for permanentemente apagada, os registos de leite dela também são (embora nós usemos Soft Delete na vaca!)
    }
}
