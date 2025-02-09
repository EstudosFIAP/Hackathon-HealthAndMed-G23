using System.ComponentModel.DataAnnotations;

namespace HealthAndMed.API.Models
{
    public class DoctorsScheduleModel
    {
        [Required(ErrorMessage = "O campo ID Medico é obrigatório.")]
        public int IdMedico { get; set; }

        [Required(ErrorMessage = "O campo Horario de Início é obrigatório.")]
        public DateTime HorarioInicio { get; set; }

        [Required(ErrorMessage = "O campo Horário Fim é obrigatório.")]
        public DateTime HorarioFim { get; set; }
    }
}