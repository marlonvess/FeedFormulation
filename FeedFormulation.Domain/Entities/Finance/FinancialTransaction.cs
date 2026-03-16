using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Finance;

public class FinancialTransaction : TenantEntity
{
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionCategory Category { get; private set; }
    public string Description { get; private set; }

    // Opcional: Se a transação for sobre uma vaca específica (ex: Venda de um bezerro)
    public Guid? AnimalId { get; private set; }

    public bool IsDeleted { get; private set; } // O nosso fiel Soft Delete!

    protected FinancialTransaction() { }

    public FinancialTransaction(Guid tenantId, DateTime date, decimal amount, TransactionType type,
                                TransactionCategory category, string description, Guid? animalId = null)
    {
        TenantId = tenantId;
        Date = date.Date;
        SetAmount(amount);
        Type = type;
        Category = category;
        Description = description.Trim();
        AnimalId = animalId;
        IsDeleted = false;
    }

    public void MarkAsDeleted() => IsDeleted = true;

    public void UpdateDetails(DateTime date, decimal amount, TransactionCategory category, string description)
    {
        Date = date.Date;
        SetAmount(amount);
        Category = category;
        Description = description.Trim();
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Transaction amount must be greater than zero.");
        Amount = amount;
    }
}
