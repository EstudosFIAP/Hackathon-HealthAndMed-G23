namespace HealthAndMed.Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Cpf {  get; set; }
        public string? Email { get; set; }
        public string? Senha {  get; set; }
        public string? TipoUsuario { get; set; }
        public DateTime Data_Criacao { get; set; }
        public DateTime Data_Modificacao { get; set; }

    }
}
