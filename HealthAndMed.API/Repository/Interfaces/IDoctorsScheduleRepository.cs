using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;

namespace HealthAndMed.API.Repository.Interfaces
{
    public interface IDoctorsScheduleRepository
    {
        IEnumerable<DoctorsScheduleDTO> ListDoctorsSchedule();
        Agenda CreateDoctorsSchedule(Agenda doctorsScheduleData);
        void UpdateDoctorsSchedule(Agenda doctorsScheduleData);
        void DeleteDoctorsSchedule(int Id);
    }
}
