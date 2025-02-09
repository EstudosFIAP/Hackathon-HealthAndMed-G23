namespace HealthAndMed.Core.Entities
{
    public class Medico
    {
        public int Id { get; set; }
        public string NumeroCrm { get; set; } = string.Empty;
        public string? Especialidade { get; set; }
        public decimal? ValorConsulta { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }

        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public ICollection<Agendamento> Agendamentos { get; set; } = [];
        public ICollection<Agenda> Agendas { get; set; } = [];
    }
}
