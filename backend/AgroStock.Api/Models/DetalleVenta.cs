namespace AgroStock.Api.Models
{
    public class DetalleVenta
    {
        public int IdDetalleVenta { get; set; }
        public decimal Cantidad { get; set; }

        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        // Queda en null si se elimina el cultivo; el historial conserva NombreProducto (RF-19)
        public int? IdInventario { get; set; }
        public Inventario? Inventario { get; set; }

        public string NombreProducto { get; set; } = string.Empty;
    }
}
