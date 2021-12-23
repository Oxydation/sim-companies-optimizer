using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimCompaniesOptimizer.Migrations
{
    public partial class RemoveUnusedProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Empty",
                table: "ExchangeTrackerEntries");

            migrationBuilder.DropColumn(
                name: "Empty2",
                table: "ExchangeTrackerEntries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Empty",
                table: "ExchangeTrackerEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Empty2",
                table: "ExchangeTrackerEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
