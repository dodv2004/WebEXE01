using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Verdict = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportCount = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportChecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reporters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reporters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReporterReportChecks",
                columns: table => new
                {
                    ReporterId = table.Column<int>(type: "int", nullable: false),
                    ReportCheckId = table.Column<int>(type: "int", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReporterReportChecks", x => new { x.ReporterId, x.ReportCheckId });
                    table.ForeignKey(
                        name: "FK_ReporterReportChecks_ReportChecks_ReportCheckId",
                        column: x => x.ReportCheckId,
                        principalTable: "ReportChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReporterReportChecks_Reporters_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Reporters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReporterReportChecks_ReportCheckId",
                table: "ReporterReportChecks",
                column: "ReportCheckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReporterReportChecks");

            migrationBuilder.DropTable(
                name: "ReportChecks");

            migrationBuilder.DropTable(
                name: "Reporters");
        }
    }
}
