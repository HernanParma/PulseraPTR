using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricasExtendidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Mediciones') AND name = 'CaloriasQuemadas')
                    ALTER TABLE [Mediciones] ADD [CaloriasQuemadas] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Mediciones') AND name = 'MinutosActividad')
                    ALTER TABLE [Mediciones] ADD [MinutosActividad] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Mediciones') AND name = 'MinutosSueno')
                    ALTER TABLE [Mediciones] ADD [MinutosSueno] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Mediciones') AND name = 'NivelEstres')
                    ALTER TABLE [Mediciones] ADD [NivelEstres] int NULL;
            ");

            // Poblar datos ficticios para métricas extendidas en registros existentes
            migrationBuilder.Sql(@"
                UPDATE Mediciones
                SET NivelEstres = ABS(CHECKSUM(NEWID())) % 61 + 20,
                    MinutosSueno = ABS(CHECKSUM(NEWID())) % 121 + 360,
                    MinutosActividad = ABS(CHECKSUM(NEWID())) % 90 + 10,
                    CaloriasQuemadas = ABS(CHECKSUM(NEWID())) % 600 + 150
                WHERE NivelEstres IS NULL;
            ");

            // Insertar lecturas de glucemia ficticias para el paciente 22
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM GlucoseReadings WHERE PacienteId = 22)
                BEGIN
                    DECLARE @base DATETIME2 = '2026-04-28T08:00:00';
                    DECLARE @i INT = 0;
                    WHILE @i < 25
                    BEGIN
                        DECLARE @hours INT = @i * 8 + (ABS(CHECKSUM(NEWID())) % 4);
                        DECLARE @dt DATETIME2 = DATEADD(HOUR, @hours, @base);
                        DECLARE @val INT = CASE
                            WHEN @i % 7 = 0 THEN ABS(CHECKSUM(NEWID())) % 30 + 55
                            WHEN @i % 5 = 0 THEN ABS(CHECKSUM(NEWID())) % 50 + 185
                            ELSE ABS(CHECKSUM(NEWID())) % 80 + 75
                        END;
                        DECLARE @lbl NVARCHAR(200) = CASE
                            WHEN @i % 3 = 0 THEN 'En ayunas'
                            WHEN @i % 3 = 1 THEN N'Después de comer'
                            ELSE 'Antes de comer'
                        END;
                        DECLARE @hash NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256',
                            CONCAT('22|', CONVERT(NVARCHAR(30), @dt, 126), '|', @val, '|', @lbl)), 2);

                        INSERT INTO GlucoseReadings (PacienteId, ReadingDateTime, DateRaw, TimeRaw, Label, GlucoseMgDl, [TimeZone], SourceFileName, Source, ImportHash, CreatedAt)
                        VALUES (22, @dt,
                                FORMAT(@dt, 'dd/MM/yyyy'), FORMAT(@dt, 'HH:mm'),
                                @lbl, @val, 'GMT-03:00', 'seed-demo.csv', 0, @hash, GETUTCDATE());

                        SET @i = @i + 1;
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloriasQuemadas",
                table: "Mediciones");

            migrationBuilder.DropColumn(
                name: "MinutosActividad",
                table: "Mediciones");

            migrationBuilder.DropColumn(
                name: "MinutosSueno",
                table: "Mediciones");

            migrationBuilder.DropColumn(
                name: "NivelEstres",
                table: "Mediciones");
        }
    }
}
