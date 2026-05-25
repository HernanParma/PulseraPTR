using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregoRangoValores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FrecuenciaCardiaca",
                table: "Mediciones",
                newName: "ValorMedicion");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Mediciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RangoValoresMedicion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TipoMedicion = table.Column<int>(type: "int", nullable: false),
                    RangoEdadMinimo = table.Column<int>(type: "int", nullable: false),
                    RangoEdadMaximo = table.Column<int>(type: "int", nullable: false),
                    ValorNormalMinimo = table.Column<int>(type: "int", nullable: false),
                    ValorNormalMaximo = table.Column<int>(type: "int", nullable: false),
                    ValorCriticoMinimo = table.Column<int>(type: "int", nullable: false),
                    ValorCriticoMaximo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangoValoresMedicion", x => x.Id);
                    table.CheckConstraint("CK_RangoValoresMedicion_RangoEdadMaximo", "[RangoEdadMaximo] <> 0");
                    table.CheckConstraint("CK_RangoValoresMedicion_RangoEdadMinimo", "[RangoEdadMinimo] <> 0");
                    table.CheckConstraint("CK_RangoValoresMedicion_ValorCriticoMaximo", "[ValorCriticoMaximo] <> 0");
                    table.CheckConstraint("CK_RangoValoresMedicion_ValorCriticoMinimo", "[ValorCriticoMinimo] <> 0");
                    table.CheckConstraint("CK_RangoValoresMedicion_ValorNormalMaximo", "[ValorNormalMaximo] <> 0");
                    table.CheckConstraint("CK_RangoValoresMedicion_ValorNormalMinimo", "[ValorNormalMinimo] <> 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RangoValoresMedicion");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Mediciones");

            migrationBuilder.RenameColumn(
                name: "ValorMedicion",
                table: "Mediciones",
                newName: "FrecuenciaCardiaca");
        }
    }
}
