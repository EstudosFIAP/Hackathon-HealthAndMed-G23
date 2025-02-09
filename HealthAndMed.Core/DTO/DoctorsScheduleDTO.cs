namespace HealthAndMed.Core.DTO
{
    public class DoctorsScheduleDTO
    {
        public int Id { get; set; }
        public int IdMedico {  get; set; }
        public string Medico { get; set; } = string.Empty;
        public string CrmMedico {  get; set; } = string.Empty;
        public string Especialidade {  get; set; } = string.Empty;
        public decimal? ValorConsulta { get; set; }
        public bool IsBloqueado { get; set; }
        public DateTime HorarioInicio { get; set; }
        public DateTime HorarioFim { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}
