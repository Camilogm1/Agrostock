namespace AgroStock.Api.Exceptions
{
    public class CultivoConStockException : ConflictoException
    {
        public CultivoConStockException(int idCultivo, decimal stock)
            : base($"No se puede eliminar ni archivar el cultivo {idCultivo}: aún tiene {stock:0.##} kg disponibles en inventario.") { }
    }
}
