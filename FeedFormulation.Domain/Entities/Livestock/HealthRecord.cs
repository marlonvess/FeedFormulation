using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Livestock;

public class HealthRecord : TenantEntity
{
    public Guid AnimalId { get; private set; }
    public virtual Animal Animal { get; private set; } = null!;

    public DateTime Date { get; private set; }
    public HealthEventType EventType { get; private set; }

    public string DiagnosisOrName { get; private set; } // Ex: "Mastite", "Vacina BVD"
    public string? Treatment { get; private set; } // Ex: "Antibiótico X 50ml"

    // Já a pensar na Fase 5 (Financeira), vamos guardar o custo do tratamento!
    public decimal? Cost { get; private set; }

    public bool IsDeleted { get; private set; } // Preparado para Soft Delete

    protected HealthRecord() { }

    public HealthRecord(Guid tenantId, Guid animalId, DateTime date, HealthEventType eventType,
                        string diagnosisOrName, string? treatment = null, decimal? cost = null)
    {
        TenantId = tenantId;
        AnimalId = animalId;
        Date = date.Date;
        EventType = eventType;
        DiagnosisOrName = diagnosisOrName.Trim();
        Treatment = treatment?.Trim();

        SetCost(cost);
        IsDeleted = false;
    }

    public void MarkAsDeleted() => IsDeleted = true;

    public void SetCost(decimal? cost)
    {
        if (cost < 0) throw new ArgumentException("Cost cannot be negative.");
        Cost = cost;
    }

    public void UpdateDetails(string diagnosis, string? treatment, decimal? cost)
    {
        DiagnosisOrName = diagnosis.Trim();
        Treatment = treatment?.Trim();
        SetCost(cost);
    }
}
