using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimCompaniesOptimizer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Transportation = table.Column<double>(type: "REAL", nullable: false),
                    ProducedAnHour = table.Column<double>(type: "REAL", nullable: false),
                    BaseSalary = table.Column<double>(type: "REAL", nullable: false),
                    ProducedFrom = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
