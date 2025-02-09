using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.API.Repository
{
    public class DoctorsScheduleRepository : IDoctorsScheduleRepository
    {
        private readonly DbHmContext _context;

        public DoctorsScheduleRepository(DbHmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Agenda CreateDoctorsSchedule(Agenda doctorsScheduleData)
        {
            if (doctorsScheduleData == null)
                throw new ArgumentException("Você precisa informar os dados da agenda para cadastro");

            doctorsScheduleData.Data_Criacao = DateTime.Now;
            doctorsScheduleData.Data_Modificacao = DateTime.Now;
            doctorsScheduleData.IsBloqueado = false;
            _context.Agenda.Add(doctorsScheduleData);
            _context.SaveChanges();

            return doctorsScheduleData;
        }

        public void DeleteDoctorsSchedule(int Id)
        {
            var doctorsSchedule = _context.Agenda.FirstOrDefault(d => d.Id == Id);
            if (doctorsSchedule == null)
                throw new NotImplementedException();

            _context.Agenda.Remove(doctorsSchedule);
            _context.SaveChanges();
        }

        public IEnumerable<DoctorsScheduleDTO> ListDoctorsSchedule()
        {
            var query = from agendas in _context.Agenda
                        select new DoctorsScheduleDTO
                        {
                            Id = agendas.Id,
                            IdMedico = agendas.IdMedico,
                            Medico = agendas.Medico.Usuario.Nome,
                            CrmMedico = agendas.Medico.NumeroCrm,
                            Especialidade = agendas.Medico.Especialidade,
                            ValorConsulta = agendas.Medico.ValorConsulta,                            
                            HorarioInicio = agendas.HorarioInicio,
                            HorarioFim = agendas.HorarioFim,
                            IsBloqueado = agendas.IsBloqueado,
                            DataCriacao = agendas.Data_Criacao,
                            DataModificacao = agendas.Data_Modificacao
                        };

            return query.Distinct().ToList();
        }

        public void UpdateDoctorsSchedule(Agenda doctorsScheduleData)
        {
            if (doctorsScheduleData == null)
                throw new ArgumentException("Você precisa informar os dados da agenda para atualização");

            var existingDoctorsSchedule = _context.Agenda.FirstOrDefault(d => d.Id == doctorsScheduleData.Id);
            if (existingDoctorsSchedule == null)
                throw new ArgumentException("Agendamento não encontrado");

            existingDoctorsSchedule.IsBloqueado = doctorsScheduleData.IsBloqueado;
            existingDoctorsSchedule.HorarioInicio = doctorsScheduleData.HorarioInicio != default ? doctorsScheduleData.HorarioInicio : existingDoctorsSchedule.HorarioInicio;
            existingDoctorsSchedule.HorarioFim = doctorsScheduleData.HorarioFim != default ? doctorsScheduleData.HorarioFim : existingDoctorsSchedule.HorarioFim;
        
            existingDoctorsSchedule.Data_Modificacao = DateTime.Now;

            _context.Entry(existingDoctorsSchedule).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}