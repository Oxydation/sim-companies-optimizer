using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimCompaniesOptimizer.Migrations
{
    public partial class ExchangeTrackerContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeTrackerEntries",
                columns: table => new
                {
                    Empty = table.Column<string>(type: "TEXT", nullable: false),
                    Empty2 = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ExchangePrices = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeTrackerEntries");
        }
    }
}
