namespace AgroStockTaller.Api.Models
{
    public class Inventario
    {
        public int IdInventario { get; set; }
        public decimal CantidadDisponible { get; set; }
        public int IdCultivo { get; set; }
    }
}
