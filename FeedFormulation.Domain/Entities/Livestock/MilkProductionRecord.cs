using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;

namespace FeedFormulation.Domain.Entities.Livestock;

/// <summary>
///
/// </summary>
public class MilkProductionRecord : TenantEntity
{
    // Relação com a Vaca
    public Guid AnimalId { get; private set; }
    public virtual Animal Animal { get; private set; } = null!;

    // Dados Principais
    public DateTime Date { get; private set; }
    public decimal VolumeInLiters { get; private set; }

    // Dados de Qualidade (Opcionais, pois nem sempre há análises diárias)
    public decimal? FatPercentage { get; private set; }
    public decimal? ProteinPercentage { get; private set; }
    public int? SomaticCellCount { get; private set; } // Contagem de Células Somáticas (CCS)

    // Construtor vazio para o Entity Framework
    protected MilkProductionRecord() { }

    public bool IsDeleted { get; private set; }

    // Construtor principal
    public MilkProductionRecord(Guid tenantId, Guid animalId, DateTime date, decimal volumeInLiters,
                                decimal? fatPercentage = null, decimal? proteinPercentage = null, int? somaticCellCount = null)
    {
        TenantId = tenantId;
        AnimalId = animalId;
        Date = date.Date; // Garantimos que guarda apenas o dia, sem horas

        SetVolume(volumeInLiters);

        FatPercentage = fatPercentage;
        ProteinPercentage = proteinPercentage;
        SomaticCellCount = somaticCellCount;

        IsDeleted = false;
    }

    // Método de atualização seguro
    public void SetVolume(decimal volume)
    {
        if (volume < 0) throw new ArgumentException("Milk volume cannot be negative.");
        VolumeInLiters = volume;
    }

    public void UpdateQuality(decimal? fat, decimal? protein, int? scc)
    {
        FatPercentage = fat;
        ProteinPercentage = protein;
        SomaticCellCount = scc;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }
}
