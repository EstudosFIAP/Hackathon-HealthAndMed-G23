namespace HealthAndMed.Core.Entities
{
    public class Agenda
    {
        public int Id { get; set; }        
        public bool IsBloqueado {  get; set; }
        public DateTime HorarioInicio { get; set; }
        public DateTime HorarioFim { get; set; }
        public DateTime Data_Criacao {  get; set; }
        public DateTime Data_Modificacao { get; set; }

        public virtual Medico? Medico { get; set; }
        public int IdMedico { get; set; }
    }
}