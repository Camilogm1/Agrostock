namespace AgroStock.Api.Exceptions
{
    public class CultivoConCosechasException : Exception
    {
        public CultivoConCosechasException(int idCultivo)
            : base($"No se puede eliminar el cultivo {idCultivo}: tiene cosechas asociadas.") { }
    }
}
