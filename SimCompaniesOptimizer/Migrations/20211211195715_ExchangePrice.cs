using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimCompaniesOptimizer.Migrations
{
    public partial class ExchangePrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CurrentExchangePrice",
                table: "Resources",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "PriceCard",
                table: "Resources",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentExchangePrice",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "PriceCard",
                table: "Resources");
        }
    }
}
