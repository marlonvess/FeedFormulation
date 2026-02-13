using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedFormulation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PriceCurrent",
                table: "Ingredients",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_IngredientNutrientProfiles_IngredientId",
                table: "IngredientNutrientProfiles",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientNutrientProfiles_Ingredients_IngredientId",
                table: "IngredientNutrientProfiles",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IngredientNutrientProfiles_Ingredients_IngredientId",
                table: "IngredientNutrientProfiles");

            migrationBuilder.DropIndex(
                name: "IX_IngredientNutrientProfiles_IngredientId",
                table: "IngredientNutrientProfiles");

            migrationBuilder.DropColumn(
                name: "PriceCurrent",
                table: "Ingredients");
        }
    }
}
