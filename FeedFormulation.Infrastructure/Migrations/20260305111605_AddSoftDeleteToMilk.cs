using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedFormulation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToMilk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MilkProductionRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MilkProductionRecords");
        }
    }
}
