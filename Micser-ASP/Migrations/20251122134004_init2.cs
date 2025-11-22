using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tbank_back_web.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "Receipts");

            migrationBuilder.AddColumn<string>(
                name: "IngredientsAmount",
                table: "Receipts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientsAmount",
                table: "Receipts");

            migrationBuilder.AddColumn<List<string>>(
                name: "Ingredients",
                table: "Receipts",
                type: "text[]",
                nullable: false);
        }
    }
}
