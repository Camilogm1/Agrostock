namespace AgroStock.Api.Exceptions
{
    public class CultivoConCosechasException : ConflictoException
    {
        public CultivoConCosechasException(int idCultivo)
            : base($"No se puede eliminar el cultivo {idCultivo}: tiene cosechas registradas. Archívelo para ocultarlo sin perder su historial.") { }
    }
}
