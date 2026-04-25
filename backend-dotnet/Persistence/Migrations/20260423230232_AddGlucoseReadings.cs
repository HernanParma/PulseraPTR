using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGlucoseReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlucoseReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    ReadingDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRaw = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeRaw = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GlucoseMgDl = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    ImportHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlucoseReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlucoseReadings_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlucoseReadings_PacienteId_ImportHash",
                table: "GlucoseReadings",
                columns: new[] { "PacienteId", "ImportHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlucoseReadings_PacienteId_ReadingDateTime",
                table: "GlucoseReadings",
                columns: new[] { "PacienteId", "ReadingDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlucoseReadings");
        }
    }
}
