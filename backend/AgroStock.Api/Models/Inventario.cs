namespace AgroStock.Api.Models
{
    public class Inventario
    {
        public int IdInventario { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        public int IdCultivo { get; set; }
        public Cultivo? Cultivo { get; set; }

        public bool HayStockSuficiente(decimal cantidad) => CantidadDisponible >= cantidad;

        public void IncrementarStock(decimal cantidad)
        {
            CantidadDisponible += cantidad;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void DescontarStock(decimal cantidad)
        {
            if (cantidad > CantidadDisponible)
                throw new Exceptions.StockInsuficienteException(IdInventario, cantidad, CantidadDisponible);

            CantidadDisponible -= cantidad;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
