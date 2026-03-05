using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Common;
using FeedFormulation.Domain.Enums;

namespace FeedFormulation.Domain.Entities.Livestock
{
    /// <summary>
    /// 
    /// </summary>
    public class Animal : TenantEntity
    {
        // 1. Identificação Básica
        public string SiaNumber { get; private set; } // Brinco SIA (ex: PT414136604)
        public string? Name { get; private set; }     // Nome (ex: Estrela)
        public string? InternalNumber { get; private set; } // Nº Casa (ex: 6604)

        // 2. Características Físicas
        public DateTime DateOfBirth { get; private set; } // Para calcular a Idade depois
        public AnimalGender Gender { get; private set; }  // Género
        public string Breed { get; private set; }         // Raça

        // 3. Organização e Produção
        public string? Group { get; private set; } // Grupo reprodutivo/produtivo (ex: "Fim de gestação")
        public string? Lot { get; private set; }   // Lote físico (ex: "A1")
        public decimal? LastMilkProduction { get; private set; } // Produção L/dia (ex: 32.5)

        // 4. Status e Genealogia (Extras importantes)
        public AnimalStatus Status { get; private set; }
        public string? SireSia { get; private set; } // Pai
        public string? DamSia { get; private set; }  // Mãe

        // Relação: Uma vaca tem vários registos de produção de leite
        public virtual ICollection<MilkProductionRecord> MilkProductions { get; private set; } = new List<MilkProductionRecord>();

        // Construtor vazio (obrigatório para o Entity Framework)
        protected Animal() { }

        // Construtor principal
        public Animal(Guid tenantId, string siaNumber, string? name, string? internalNumber,
                      DateTime dateOfBirth, AnimalGender gender, string breed)
        {
            TenantId = tenantId;
            SiaNumber = siaNumber.Trim().ToUpper();
            Name = name?.Trim();
            InternalNumber = internalNumber?.Trim();
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Breed = breed.Trim();
            Status = AnimalStatus.Active;
        }

        // Métodos para atualizar os dados (Clean Architecture)
        public void UpdateManagementInfo(string? group, string? lot)
        {
            Group = group?.Trim();
            Lot = lot?.Trim();
        }

        public void UpdateProduction(decimal milkProduction)
        {
            LastMilkProduction = milkProduction;
        }

        public void ChangeStatus(AnimalStatus newStatus)
        {
            Status = newStatus;
        }
    }
}
