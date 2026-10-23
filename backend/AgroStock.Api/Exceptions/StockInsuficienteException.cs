namespace AgroStock.Api.Exceptions
{
    public class StockInsuficienteException : Exception
    {
        public int IdInventario { get; }
        public decimal CantidadSolicitada { get; }
        public decimal CantidadDisponible { get; }

        public StockInsuficienteException(int idInventario, decimal solicitada, decimal disponible)
            : base($"Stock insuficiente: solicitó {solicitada:0.##} kg y solo hay {disponible:0.##} kg disponibles.")
        {
            IdInventario = idInventario;
            CantidadSolicitada = solicitada;
            CantidadDisponible = disponible;
        }
    }
}
