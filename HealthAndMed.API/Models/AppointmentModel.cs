using System.ComponentModel.DataAnnotations;

namespace HealthAndMed.API.Models
{
    public class AppointmentModel
    {
        [Required(ErrorMessage = "O campo ID Paciente é obrigatório.")]
        public int IdPaciente { get; set; }

        [Required(ErrorMessage = "O campo ID Medico é obrigatório.")]
        public int IdMedico { get; set; }

        [Required(ErrorMessage = "O campo ID Agenda é obrigatório.")]
        public int IdAgenda { get; set; }
    }
}
