using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGlucoseImportBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GlucoseReadings') AND name = 'EmailReceivedAtUtc')
                    ALTER TABLE [GlucoseReadings] ADD [EmailReceivedAtUtc] datetime2 NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GlucoseReadings') AND name = 'ImportBatchId')
                    ALTER TABLE [GlucoseReadings] ADD [ImportBatchId] nvarchar(128) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GlucoseReadings_ImportBatchId' AND object_id = OBJECT_ID('GlucoseReadings'))
                    CREATE INDEX [IX_GlucoseReadings_ImportBatchId] ON [GlucoseReadings] ([ImportBatchId]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GlucoseReadings_ImportBatchId",
                table: "GlucoseReadings");

            migrationBuilder.DropColumn(
                name: "EmailReceivedAtUtc",
                table: "GlucoseReadings");

            migrationBuilder.DropColumn(
                name: "ImportBatchId",
                table: "GlucoseReadings");
        }
    }
}
