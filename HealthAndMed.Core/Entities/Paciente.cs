namespace HealthAndMed.Core.Entities
{
    public class Paciente
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }

        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    }
}