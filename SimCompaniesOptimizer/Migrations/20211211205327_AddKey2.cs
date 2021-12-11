using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimCompaniesOptimizer.Migrations
{
    public partial class AddKey2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Timestamp",
                table: "ExchangeTrackerEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExchangeTrackerEntries",
                table: "ExchangeTrackerEntries",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExchangeTrackerEntries",
                table: "ExchangeTrackerEntries");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Timestamp",
                table: "ExchangeTrackerEntries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");
        }
    }
}
