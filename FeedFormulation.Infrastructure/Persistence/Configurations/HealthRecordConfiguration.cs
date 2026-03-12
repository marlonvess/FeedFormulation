using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Livestock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedFormulation.Infrastructure.Persistence.Configurations;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.ToTable("HealthRecords");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.DiagnosisOrName).IsRequired().HasMaxLength(100);
        builder.Property(h => h.Treatment).HasMaxLength(250);

        // Guardar o custo com precisão de Euros (ex: 99999.99)
        builder.Property(h => h.Cost).HasPrecision(10, 2);

        // A Ponte: Um animal tem muitos registos de saúde
        builder.HasOne(h => h.Animal)
            .WithMany(a => a.HealthRecords)
            .HasForeignKey(h => h.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}