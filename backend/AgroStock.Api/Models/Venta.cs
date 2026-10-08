namespace AgroStock.Api.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }

        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Composición: un detalle no existe sin su venta
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
