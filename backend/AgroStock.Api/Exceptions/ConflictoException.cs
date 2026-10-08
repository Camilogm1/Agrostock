namespace AgroStock.Api.Exceptions
{
    // La operación choca con el estado actual de los datos (se responde 409).
    public class ConflictoException : Exception
    {
        public ConflictoException(string mensaje) : base(mensaje) { }
    }
}
