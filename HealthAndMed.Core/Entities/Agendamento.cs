namespace HealthAndMed.Core.Entities
{
    public class Agendamento
    {
        public int Id { get; set; }               
        public string? StatusAgendamento { get; set; }
        public string? MotivoCancelamento {  get; set; }
        public DateTime Data_Criacao { get; set; }
        public DateTime Data_Modificacao { get; set; }

        public Paciente? Paciente { get; set; }
        public int IdPaciente { get; set; }

        public virtual Medico? Medico { get; set; }
        public int IdMedico { get; set; }

        public Agenda? Agenda { get; set; }
        public int IdAgenda { get; set; }
    }
}