namespace AgroStock.Api.Exceptions
{
    public class StockInsuficienteException : Exception
    {
        public int IdInventario { get; }
        public decimal CantidadSolicitada { get; }
        public decimal CantidadDisponible { get; }

        public StockInsuficienteException(int idInventario, decimal solicitada, decimal disponible)
            : base($"Stock insuficiente para el inventario {idInventario}: solicitado {solicitada}, disponible {disponible}.")
        {
            IdInventario = idInventario;
            CantidadSolicitada = solicitada;
            CantidadDisponible = disponible;
        }
    }
}
