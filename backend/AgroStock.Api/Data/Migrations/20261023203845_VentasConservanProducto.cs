using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroStock.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class VentasConservanProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Inventarios_IdInventario",
                table: "DetallesVenta");

            migrationBuilder.AlterColumn<int>(
                name: "IdInventario",
                table: "DetallesVenta",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "NombreProducto",
                table: "DetallesVenta",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Las ventas que ya existían toman el nombre de su cultivo
            migrationBuilder.Sql(@"
                UPDATE DetallesVenta d
                JOIN Inventarios i ON i.IdInventario = d.IdInventario
                JOIN Cultivos c ON c.IdCultivo = i.IdCultivo
                SET d.NombreProducto = c.Nombre;");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Inventarios_IdInventario",
                table: "DetallesVenta",
                column: "IdInventario",
                principalTable: "Inventarios",
                principalColumn: "IdInventario",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Inventarios_IdInventario",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "NombreProducto",
                table: "DetallesVenta");

            migrationBuilder.AlterColumn<int>(
                name: "IdInventario",
                table: "DetallesVenta",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Inventarios_IdInventario",
                table: "DetallesVenta",
                column: "IdInventario",
                principalTable: "Inventarios",
                principalColumn: "IdInventario",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
