using System.ComponentModel.DataAnnotations;

namespace HealthAndMed.API.Models
{
    public class DoctorModel
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string? NumeroCrm { get; set; }
    }
}
