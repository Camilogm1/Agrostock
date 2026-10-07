namespace AgroStock.Api.Models
{
    public class DetalleVenta
    {
        public int IdDetalleVenta { get; set; }
        public decimal Cantidad { get; set; }

        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        public int IdInventario { get; set; }
        public Inventario? Inventario { get; set; }
    }
}
