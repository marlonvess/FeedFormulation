using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedFormulation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSolverRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolverRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormulaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunStatus = table.Column<int>(type: "integer", nullable: false),
                    SolverName = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: true),
                    DiagnosticMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolverRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolverRunLineResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolverRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InclusionPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    CostContribution = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolverRunLineResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolverRunLineResults_SolverRuns_SolverRunId",
                        column: x => x.SolverRunId,
                        principalTable: "SolverRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolverRunNutrientResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolverRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    NutrientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievedValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MinRequired = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxAllowed = table.Column<decimal>(type: "numeric", nullable: true),
                    IsBinding = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolverRunNutrientResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolverRunNutrientResults_SolverRuns_SolverRunId",
                        column: x => x.SolverRunId,
                        principalTable: "SolverRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolverRunLineResults_SolverRunId",
                table: "SolverRunLineResults",
                column: "SolverRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SolverRunNutrientResults_SolverRunId",
                table: "SolverRunNutrientResults",
                column: "SolverRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolverRunLineResults");

            migrationBuilder.DropTable(
                name: "SolverRunNutrientResults");

            migrationBuilder.DropTable(
                name: "SolverRuns");
        }
    }
}
