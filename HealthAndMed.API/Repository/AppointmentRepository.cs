using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.API.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DbHmContext _context;

        public AppointmentRepository(DbHmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Agendamento CreateAppointment(Agendamento appointmentData)
        {

            if (appointmentData == null)
                throw new ArgumentException("Você precisa informar os dados do agendamento para cadastro");

            appointmentData.Data_Criacao = DateTime.Now;
            appointmentData.Data_Modificacao = DateTime.Now;
            appointmentData.StatusAgendamento = "Pendente";
            _context.Agendamento.Add(appointmentData);
            _context.SaveChanges();

            return appointmentData;
        }

        public void DeleteAppointment(int Id)
        {
            var appointment = _context.Agendamento.FirstOrDefault(a => a.Id == Id);
            if (appointment == null)
                throw new NotImplementedException();

            _context.Agendamento.Remove(appointment);
            _context.SaveChanges();
        }

        public IEnumerable<AppointmentDTO> ListAppointments()
        {
            var query = from agendamentos in _context.Agendamento
                        select new AppointmentDTO
                        {
                            Id = agendamentos.Id,
                            IdPaciente = agendamentos.IdPaciente,
                            Paciente = agendamentos.Paciente.Usuario.Nome,
                            IdMedico = agendamentos.IdMedico,
                            Medico = agendamentos.Medico.Usuario.Nome,
                            IdAgenda = agendamentos.IdAgenda,
                            DataAgenda = agendamentos.Agenda.HorarioInicio,
                            StatusAgendamento = agendamentos.StatusAgendamento!,
                            MotivoCancelamento = agendamentos.MotivoCancelamento!,
                            DataCriacao = agendamentos.Data_Criacao,
                            DataModificacao = agendamentos.Data_Modificacao
                        };

            return query.Distinct().ToList();
        }

        public void UpdateAppointment(Agendamento appointmentData)
        {
            if (appointmentData == null)
                throw new ArgumentException("Você precisa informar os dados do agendamento para atualização");

            var existingAppointment = _context.Agendamento.FirstOrDefault(a =>a.Id == appointmentData.Id);
            if (existingAppointment == null)
                throw new ArgumentException("Agendamento não encontrado");

            existingAppointment.IdPaciente = appointmentData.IdPaciente > 0 ? appointmentData.IdPaciente : existingAppointment.IdPaciente;
            existingAppointment.IdMedico = appointmentData.IdMedico > 0 ? appointmentData.IdMedico : existingAppointment.IdMedico;
            existingAppointment.IdAgenda = appointmentData.IdAgenda > 0 ? appointmentData.IdAgenda : existingAppointment.IdAgenda;
            existingAppointment.StatusAgendamento = appointmentData.StatusAgendamento != null ? appointmentData.StatusAgendamento : existingAppointment.StatusAgendamento;
            existingAppointment.MotivoCancelamento = appointmentData.MotivoCancelamento !=null ? appointmentData.MotivoCancelamento : existingAppointment.MotivoCancelamento;
            existingAppointment.Data_Modificacao = DateTime.Now;
            
            _context.Entry(existingAppointment).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
