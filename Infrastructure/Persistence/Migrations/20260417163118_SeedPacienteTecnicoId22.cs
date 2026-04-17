using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPacienteTecnicoId22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pacientes.Id es IDENTITY: InsertData de EF fallaría sin IDENTITY_INSERT.
            // Idempotente: si ya existe Id=22 (p. ej. insert manual), no duplica.
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [Pacientes] WHERE [Id] = 22)
                BEGIN
                    SET IDENTITY_INSERT [Pacientes] ON;
                    INSERT INTO [Pacientes] ([Id], [Nombre], [Edad], [Dni], [ContactoEmergencia], [Observaciones], [Activo])
                    VALUES (22, N'Reloj en tiempo real', 1, NULL, N'N/A (paciente técnico)', N'Paciente técnico para integración con app Android en tiempo real', 1);
                    SET IDENTITY_INSERT [Pacientes] OFF;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Pacientes] WHERE [Id] = 22");
        }
    }
}
