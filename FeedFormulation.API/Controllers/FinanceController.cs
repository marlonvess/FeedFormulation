using FeedFormulation.Api.Services;
using FeedFormulation.Domain.Entities.Finance;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager, Admin")] // 🔒 BLOQUEIO TOTAL: Só Gestores e Admins podem ver/mexer nas finanças
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public FinanceController(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private Guid TenantId => _currentUser.GetTenantId();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var transaction = new FinancialTransaction(
            TenantId, dto.Date, dto.Amount, dto.Type, dto.Category, dto.Description, dto.AnimalId
        );
        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Transaction successfully registered!", id = transaction.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _context.FinancialTransactions
            .Where(t => t.TenantId == TenantId && !t.IsDeleted)
            .OrderByDescending(t => t.Date)
            .Select(t => new { t.Id, date = t.Date.ToString("yyyy-MM-dd"), amount = t.Amount, type = t.Type.ToString(), category = t.Category.ToString(), description = t.Description, animalId = t.AnimalId })
            .ToListAsync();
        return Ok(transactions);
    }

    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int? year, [FromQuery] int? month)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var transactions = await _context.FinancialTransactions
            .Where(t => t.TenantId == TenantId && !t.IsDeleted && t.Date.Year == targetYear && t.Date.Month == targetMonth)
            .ToListAsync();

        var totalRevenue = transactions.Where(t => t.Type == TransactionType.Revenue).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var profit = totalRevenue - totalExpense;

        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new { category = g.Key.ToString(), totalAmount = Math.Round(g.Sum(t => t.Amount), 2) })
            .OrderByDescending(c => c.totalAmount).ToList();

        return Ok(new { Period = $"{targetYear}-{targetMonth:D2}", TotalRevenue = Math.Round(totalRevenue, 2), TotalExpense = Math.Round(totalExpense, 2), NetProfit = Math.Round(profit, 2), ExpensesByCategory = expensesByCategory });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var transaction = await _context.FinancialTransactions.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == TenantId && !t.IsDeleted);
        if (transaction == null) return NotFound();
        transaction.MarkAsDeleted();
        await _context.SaveChangesAsync();
        return Ok(new { message = "Transaction deleted." });
    }
}

public record CreateTransactionDto(DateTime Date, decimal Amount, TransactionType Type, TransactionCategory Category, string Description, Guid? AnimalId);