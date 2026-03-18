using FeedFormulation.Domain.Entities.Finance;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public FinanceController(AppDbContext context)
    {
        _context = context;
    }

    // 1. POST: register a new financial transaction (revenue or expense)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var transaction = new FinancialTransaction(
            _tenantId, dto.Date, dto.Amount, dto.Type, dto.Category, dto.Description, dto.AnimalId
        );

        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Transaction successfully registered!", id = transaction.Id });
    }

    // 2. GET: Transactions List (For the Transactions Tab in the Dashboard)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _context.FinancialTransactions
            .Where(t => t.TenantId == _tenantId && !t.IsDeleted)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                id = t.Id,
                date = t.Date.ToString("yyyy-MM-dd"),
                amount = t.Amount,
                type = t.Type.ToString(),
                category = t.Category.ToString(),
                description = t.Description,
                animalId = t.AnimalId
            })
            .ToListAsync();

        return Ok(transactions);
    }

    // ==========================================
    // --- BUSINESS INTELLIGENCE (BI) ---
    // ==========================================

    // 3. GET: Mensal Summary (For the BI Tab in the Dashboard)
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int? year, [FromQuery] int? month)
    {
        // If year or month are not provided, default to the current month and year
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        // Get all transactions for the specified month and year
        var transactions = await _context.FinancialTransactions
            .Where(t => t.TenantId == _tenantId && !t.IsDeleted && t.Date.Year == targetYear && t.Date.Month == targetMonth)
            .ToListAsync();

        // Do some basic calculations for the monthly summary
        var totalRevenue = transactions.Where(t => t.Type == TransactionType.Revenue).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var profit = totalRevenue - totalExpense;

        // Group expenses by category for a more detailed breakdown in the BI dashboard
        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                category = g.Key.ToString(),
                totalAmount = Math.Round(g.Sum(t => t.Amount), 2)
            })
            .OrderByDescending(c => c.totalAmount) // sort by highest expense category
            .ToList();

        return Ok(new
        {
            Period = $"{targetYear}-{targetMonth:D2}",
            TotalRevenue = Math.Round(totalRevenue, 2),
            TotalExpense = Math.Round(totalExpense, 2),
            NetProfit = Math.Round(profit, 2), // Net Profit = Total Revenue - Total Expense
            ExpensesByCategory = expensesByCategory
        });
    }

    // ==========================================
    // --- SOFT DELETE ---
    // ==========================================

    // 4. DELETE: Soft delete a transaction by marking it as deleted instead of removing it from the database.
    // This allows us to maintain historical data for BI purposes while keeping the active transactions list clean. The transaction will be excluded from all queries that filter out deleted records.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var transaction = await _context.FinancialTransactions
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == _tenantId && !t.IsDeleted);

        if (transaction == null) return NotFound("Transaction not found or already deleted.");

        transaction.MarkAsDeleted();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Transaction deleted (Soft Delete)." });
    }
}

// ==========================================
// --- DTO ---
// ==========================================

public record CreateTransactionDto(
    DateTime Date,
    decimal Amount,
    TransactionType Type, // 1 = Profit , 2 = expense
    TransactionCategory Category, // 1 to 7 Milk, Animals, Ration, Vet, Salary, Equip, Others)
    string Description,
    Guid? AnimalId // Opcional If the transaction is related to a specific animal, we can link it using the AnimalId. This allows for more detailed tracking and analysis of expenses and revenues associated with individual animals in the herd.
);