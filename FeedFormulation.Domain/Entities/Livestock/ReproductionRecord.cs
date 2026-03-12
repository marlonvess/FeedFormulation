using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Livestock;

public class ReproductionRecord : TenantEntity
{
    public Guid AnimalId { get; private set; }
    public virtual Animal Animal { get; private set; } = null!;

    public DateTime Date { get; private set; }
    public ReproductionEventType EventType { get; private set; }

    // Dados específicos (podem ser nulos dependendo do tipo de evento)
    public string? SireId { get; private set; } // ID/Nome do Touro (usado na Inseminação)
    public bool? IsPregnant { get; private set; } // Resultado (usado no Diagnóstico de Gestação)
    public string? Notes { get; private set; } // Observações (ex: "Parto distócico", "Celo muito visível")

    public bool IsDeleted { get; private set; } // Preparado para Soft Delete!

    protected ReproductionRecord() { }

    public ReproductionRecord(Guid tenantId, Guid animalId, DateTime date, ReproductionEventType eventType,
                              string? sireId = null, bool? isPregnant = null, string? notes = null)
    {
        TenantId = tenantId;
        AnimalId = animalId;
        Date = date.Date;
        EventType = eventType;
        SireId = sireId?.Trim();
        IsPregnant = isPregnant;
        Notes = notes?.Trim();
        IsDeleted = false;
    }

    public void MarkAsDeleted() => IsDeleted = true;

    public void UpdateDetails(string? sireId, bool? isPregnant, string? notes)
    {
        SireId = sireId?.Trim();
        IsPregnant = isPregnant;
        Notes = notes?.Trim();
    }
}