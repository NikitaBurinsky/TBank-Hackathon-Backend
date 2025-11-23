using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tbank_back_web.Migrations
{
    /// <inheritdoc />
    public partial class init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "Receipts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalCarbs",
                table: "Receipts",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalFat",
                table: "Receipts",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "TotalKcal",
                table: "Receipts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "TotalProtein",
                table: "Receipts",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalCarbs",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalFat",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalKcal",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalProtein",
                table: "Receipts");
        }
    }
}
