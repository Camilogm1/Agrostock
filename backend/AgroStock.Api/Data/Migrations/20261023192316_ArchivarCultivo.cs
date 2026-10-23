using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroStock.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ArchivarCultivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Cultivos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true); // los cultivos existentes quedan activos
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Cultivos");
        }
    }
}
