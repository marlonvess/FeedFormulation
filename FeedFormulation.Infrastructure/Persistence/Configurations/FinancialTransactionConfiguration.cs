using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Finance;
using FeedFormulation.Domain.Entities.Livestock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedFormulation.Infrastructure.Persistence.Configurations;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions");

        builder.HasKey(f => f.Id);

        // Garantir precisão financeira: 12 dígitos no total, 2 casas decimais (até 9.999.999.999,99 €)
        builder.Property(f => f.Amount).HasPrecision(12, 2).IsRequired();

        builder.Property(f => f.Description).HasMaxLength(500);

        // A Ponte com o Animal (Opcional)
        // Regra de Ouro: Se a vaca for apagada, o registo financeiro NÃO pode ser apagado! 
        // O dinheiro já entrou ou já saiu. Por isso usamos SetNull.
        builder.HasOne<Animal>()
            .WithMany()
            .HasForeignKey(f => f.AnimalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}