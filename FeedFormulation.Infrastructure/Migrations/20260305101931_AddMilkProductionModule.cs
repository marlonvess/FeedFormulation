using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedFormulation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMilkProductionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MilkProductionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VolumeInLiters = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    FatPercentage = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    ProteinPercentage = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    SomaticCellCount = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkProductionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilkProductionRecords_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MilkProductionRecords_AnimalId",
                table: "MilkProductionRecords",
                column: "AnimalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MilkProductionRecords");
        }
    }
}
