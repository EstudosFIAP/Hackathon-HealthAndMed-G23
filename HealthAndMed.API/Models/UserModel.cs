using FIAP_TC.Contact.Api.Extensions.CustomDataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace HealthAndMed.API.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O campo CPF é obrigatório.")]        
        public string? Cpf { get; set; }

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [CustomEmailAddress(ErrorMessage = "Email inválido, informe um endereço válido e tente novamente.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        public string? Senha { get; set; }

        [Required(ErrorMessage = "O campo Tipo Usuario é obrigatório.")]
        public string? TipoUsuario { get; set; }
      
        // Campos adicionais para médicos
        public decimal? ValorConsulta { get; set; }
        public string? Especialidade { get; set; }
        public string? NumeroCrm { get; set; }
    }
}