namespace HealthAndMed.Core.DTO
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int IdPaciente { get; set; }
        public string Paciente { get; set; } = string.Empty;
        public int IdMedico { get; set; }
        public string Medico { get; set; } = string.Empty;
        public int IdAgenda { get; set; }
        public DateTime DataAgenda { get; set; }
        public string StatusAgendamento { get; set; } = string.Empty;
        public string MotivoCancelamento { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}
