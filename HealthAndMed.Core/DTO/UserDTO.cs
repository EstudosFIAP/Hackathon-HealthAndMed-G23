namespace HealthAndMed.Core.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty ;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}