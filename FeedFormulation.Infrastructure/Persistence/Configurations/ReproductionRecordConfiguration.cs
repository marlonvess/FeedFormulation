using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Livestock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedFormulation.Infrastructure.Persistence.Configurations;

public class ReproductionRecordConfiguration : IEntityTypeConfiguration<ReproductionRecord>
{
    public void Configure(EntityTypeBuilder<ReproductionRecord> builder)
    {
        builder.ToTable("ReproductionRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.SireId).HasMaxLength(50);
        builder.Property(r => r.Notes).HasMaxLength(500);

        // A Ponte: Um animal tem muitos registos de reprodução
        builder.HasOne(r => r.Animal)
            .WithMany(a => a.ReproductionRecords)
            .HasForeignKey(r => r.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}