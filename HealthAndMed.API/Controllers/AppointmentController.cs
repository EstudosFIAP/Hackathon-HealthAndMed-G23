using HealthAndMed.API.Models;
using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAndMed.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorsScheduleRepository _doctorsScheduleRepository;

        /// <summary>
        /// Construtor do AppointmentController.
        /// </summary>
        /// <param name="appointmentRepository">Repositório de agendamentos.</param>
        public AppointmentController(IAppointmentRepository appointmentRepository, 
            IDoctorRepository doctorRepository, 
            IPatientRepository patientRepository, 
            IDoctorsScheduleRepository doctorsScheduleRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _doctorsScheduleRepository = doctorsScheduleRepository;
        }

        /// <summary>
        /// Listar todos os agendamentos
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "Paciente,Médico,Administrador")]
        [HttpGet]
        public IActionResult GetAppointment()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userType = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = userType == "Administrador"
                    ? _appointmentRepository.ListAppointments().ToList()
                    : userType == "Médico"
                        ? _appointmentRepository.ListAppointments()
                            .Where(a => _doctorRepository.GetAllDoctors()
                                .Any(d => d.Id == a.IdMedico && d.IdUsuario == userId)).ToList()
                        : _appointmentRepository.ListAppointments()
                            .Where(a => _patientRepository.GetPatientByUserId(userId)?.Id == a.IdPaciente).ToList();


                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Criar agendamento
        /// </summary>
        /// <param name="inputAppointment"></param>
        /// <returns></returns>
        [Authorize(Roles = "Paciente,Administrador")]
        [HttpPost]
        public IActionResult CreateAppointment([FromBody] AppointmentModel inputAppointment)
        {
            try
            {   
                var agenda = _doctorsScheduleRepository.ListDoctorsSchedule().FirstOrDefault(a => a.Id == inputAppointment.IdAgenda);

                if (agenda == null)
                    return NotFound("Agenda não encontrada");

                if (agenda.IsBloqueado == false)
                {                    
                    _doctorsScheduleRepository.UpdateDoctorsSchedule(
                        new Agenda()
                        {
                            Id = agenda.Id,
                            IsBloqueado = true
                        }
                    );

                    var agendamento = new Agendamento()
                    {
                        IdPaciente = inputAppointment.IdPaciente,
                        IdAgenda = inputAppointment.IdAgenda,
                        IdMedico = inputAppointment.IdMedico
                    };

                    var agendamentoCriado = _appointmentRepository.CreateAppointment(agendamento);

                    return Ok(new { message = "Agendamento criado com sucesso!" });
                }
                else
                {
                    return BadRequest("A agenda selecionada já está reservada e não poderá ser utilizada, por favor, selecione outra opção e tente novamente.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Atualizar agendamento cadastrado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inputAppointment"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public IActionResult UpdateAppointment(int id, [FromBody] AppointmentModel inputAppointment)
        {
            try
            {
                var agendamentoExistente = _appointmentRepository.ListAppointments().FirstOrDefault(a => a.Id == id);
                if (agendamentoExistente == null)
                {
                    return NotFound("Agendamento não encontrado");
                }

                var agendamento = new Agendamento()
                {
                    Id = agendamentoExistente.Id,
                    IdPaciente = inputAppointment.IdPaciente,
                    IdAgenda = inputAppointment.IdAgenda,
                    IdMedico = inputAppointment.IdMedico
                };
                _appointmentRepository.UpdateAppointment(agendamento);
                return Ok(new { message = "Agendamento atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancelar agendamento do respectivo paciente
        /// </summary>
        /// <param name="id"></param>
        /// <param name="justificativa"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "Paciente,Administrador")]
        [HttpPut("{id}/cancel")]
        public IActionResult CancelAppointment(int id, [FromBody] string justificativa)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userType = User.FindFirst(ClaimTypes.Role)?.Value;

                var appointment = userType == "Administrador"
                    ? _appointmentRepository.ListAppointments().FirstOrDefault(a => a.Id == id)
                    : _appointmentRepository.ListAppointments().FirstOrDefault(a => a.Id == id && a.IdPaciente == _patientRepository.GetPatientByUserId(userId)?.Id);

                if (appointment == null)
                {
                    return NotFound("Consulta não encontrada ou não pertence ao paciente autenticado.");
                }

                if (appointment.StatusAgendamento != "Pendente" && appointment.StatusAgendamento != "Agendado")
                {
                    return BadRequest("A consulta não pode ser cancelada, pois não está no status 'Pendente' ou 'Agendado'.");
                }

                var agendamento = new Agendamento()
                {
                    Id = appointment.Id,
                    IdPaciente = appointment.IdPaciente,
                    IdAgenda = appointment.IdAgenda,
                    IdMedico = appointment.IdMedico,
                    StatusAgendamento = "Cancelado",
                    MotivoCancelamento = justificativa,
                    Data_Modificacao = DateTime.Now
                };

                _appointmentRepository.UpdateAppointment(agendamento);

                _doctorsScheduleRepository.UpdateDoctorsSchedule(
                        new Agenda()
                        {
                            Id = appointment.IdAgenda,
                            IsBloqueado = false
                        }
                );

                return Ok(new { message = "Consulta cancelada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualizar o status do agendamento
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "Médico,Administrador")]
        [HttpPut("{id}/status")]
        public IActionResult UpdateAppointmentStatus(int id, [FromBody] string status)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userType = User.FindFirst(ClaimTypes.Role)?.Value;

                var appointment = userType == "Médico"
                    ? _appointmentRepository.ListAppointments()
                        .FirstOrDefault(a => a.Id == id && _doctorRepository.GetAllDoctors()
                            .Any(d => d.Id == a.IdMedico && d.IdUsuario == userId))
                    : _appointmentRepository.ListAppointments()
                        .FirstOrDefault(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound("Consulta não encontrada ou não pertence ao usuário autenticado.");
                }

                if (appointment.StatusAgendamento != "Pendente")
                {
                    return BadRequest("A consulta não pode ser atualizada, pois não está no status 'Pendente'.");
                }

                if (status.ToLower() == "aceito")
                {
                    appointment.StatusAgendamento = "Agendado";
                }
                else if (status.ToLower() == "recusado")
                {
                    appointment.StatusAgendamento = "Recusado";
                }
                else
                {
                    return BadRequest("Status inválido. Use 'Aceito' ou 'Recusado'.");
                }

                appointment.DataModificacao = DateTime.Now;

                var agendamento = new Agendamento()
                {
                    Id = appointment.Id,
                    IdPaciente = appointment.IdPaciente,
                    IdAgenda = appointment.IdAgenda,
                    IdMedico = appointment.IdMedico,
                    StatusAgendamento = appointment.StatusAgendamento,
                    MotivoCancelamento = appointment.MotivoCancelamento,
                    Data_Criacao = appointment.DataCriacao,
                    Data_Modificacao = appointment.DataModificacao
                };

                _appointmentRepository.UpdateAppointment(agendamento);

                return Ok(new { message = $"Status da consulta alterado para {status} com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        /// <summary>
        /// Excluir agendamento por id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public IActionResult DeleteAppointment(int id)
        {
            try
            {
                var agendamentoExistente = _appointmentRepository.ListAppointments().FirstOrDefault(a => a.Id == id);
                if (agendamentoExistente == null)
                {
                    return NotFound("Agendamento não encontrado");
                }
                _appointmentRepository.DeleteAppointment(id);
                return Ok(new { message = "Agendamento excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
