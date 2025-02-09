namespace HealthAndMed.Core.DTO
{
    public class DoctorDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string NumeroCrm { get; set; } = string.Empty;
        public string Especialidade { get; set; } = string.Empty;
        public decimal ValorConsulta { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}
