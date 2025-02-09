using HealthAndMed.Core.Entities;

namespace HealthAndMed.API.Repository.Interfaces
{
    public interface IDoctorRepository
    {
        Medico CreateDoctor(Medico doctorData);
        Medico? GetDoctorByUserId(int userId);
        IEnumerable<Medico> GetAllDoctors();
    }
}
