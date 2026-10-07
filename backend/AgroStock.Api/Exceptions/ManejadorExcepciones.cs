using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AgroStock.Api.Exceptions
{
    // Traduce las excepciones de los servicios a respuestas HTTP con el formato
    // { "mensaje": "..." } que espera el frontend.
    public class ManejadorExcepciones : IExceptionHandler
    {
        private readonly ILogger<ManejadorExcepciones> _logger;
        public ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var (status, mensaje) = exception switch
            {
                ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                ConflictoException ex => (StatusCodes.Status409Conflict, ex.Message),
                StockInsuficienteException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                    "Los datos cambiaron mientras se procesaba la operación. Intente de nuevo."),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado en el servidor.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Error no controlado");

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new { mensaje }, cancellationToken);
            return true;
        }
    }
}
