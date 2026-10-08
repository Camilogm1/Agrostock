using System.ComponentModel.DataAnnotations;

namespace AgroStock.Api.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string NumeroIdentificacion { get; set; } = string.Empty; // {unique} RNF-09
    }
}
