using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;

namespace HealthAndMed.API.Repository.Interfaces;

public interface IAppointmentRepository
{
    IEnumerable<AppointmentDTO> ListAppointments();
    Agendamento CreateAppointment(Agendamento appointmentData);
    void UpdateAppointment(Agendamento appointmentData);
    void DeleteAppointment(int Id);
}