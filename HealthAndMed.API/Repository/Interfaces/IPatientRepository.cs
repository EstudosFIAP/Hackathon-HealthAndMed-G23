using HealthAndMed.Core.Entities;

namespace HealthAndMed.API.Repository.Interfaces
{
    public interface IPatientRepository
    {
        Paciente CreatePatient(Paciente patientData);
        Paciente? GetPatientByUserId(int userId);
    }
}
