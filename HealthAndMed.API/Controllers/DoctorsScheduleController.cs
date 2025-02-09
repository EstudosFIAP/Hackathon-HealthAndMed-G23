using HealthAndMed.API.Models;
using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAndMed.API.Controllers
{
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsScheduleController : ControllerBase
    {
        private readonly IDoctorsScheduleRepository _doctorsScheduleRepository;
        private readonly IDoctorRepository _doctorRepository;

        /// <summary>
        /// Construtor do DoctorsScheduleController.
        /// </summary>
        /// <param name="doctorsScheduleRepository">Repositório de agendas dos médicos.</param>
        public DoctorsScheduleController(IDoctorsScheduleRepository doctorsScheduleRepository, IDoctorRepository doctorRepository)
        {
            _doctorsScheduleRepository = doctorsScheduleRepository;
            _doctorRepository = doctorRepository;
        }

        /// <summary>
        /// Listar todas as agendas dos médicos
        /// </summary>
        /// <returns>Uma lista de agendas dos médicos</returns>
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "Médico,Administrador")]
        [HttpGet]
        public IActionResult GetDoctorsSchedule()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userType = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = userType == "Médico"
                    ? _doctorsScheduleRepository.ListDoctorsSchedule()
                        .Where(a => _doctorRepository.GetAllDoctors()
                            .Any(d => d.Id == a.IdMedico && d.IdUsuario == userId)).ToList()
                : _doctorsScheduleRepository.ListDoctorsSchedule();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Criar agenda
        /// </summary>
        /// <param name="inputDoctorsSchedule">Modelo com os dados da agenda do médico</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Médico,Administrador")]
        [HttpPost]
        public IActionResult CreateDoctorsSchedule([FromBody] DoctorsScheduleModel inputDoctorsSchedule)
        {
            try
            {
                var agenda = new Agenda()
                {
                    IdMedico = inputDoctorsSchedule.IdMedico,
                    HorarioInicio = inputDoctorsSchedule.HorarioInicio,
                    HorarioFim = inputDoctorsSchedule.HorarioFim
                };

                var agendaCriada = _doctorsScheduleRepository.CreateDoctorsSchedule(agenda);
                return Ok(new { message = "Agenda criada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Atualizar agenda cadastrada
        /// </summary>
        /// <param name="id">Identificador da agenda</param>
        /// <param name="inputDoctorsSchedule">Modelo com os dados atualizados da agenda</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Médico,Administrador")]
        [HttpPut("{id}")]
        public IActionResult UpdateDoctorsSchedule(int id, [FromBody] DoctorsScheduleModel inputDoctorsSchedule)
        {
            try
            {
                var agendaExistente = _doctorsScheduleRepository.ListDoctorsSchedule().FirstOrDefault(d => d.Id == id);
                if (agendaExistente == null)
                {
                    return NotFound("Agenda não encontrada");
                }

                var agenda = new Agenda()
                {
                    Id = agendaExistente.Id,
                    HorarioInicio = inputDoctorsSchedule.HorarioInicio,
                    HorarioFim = inputDoctorsSchedule.HorarioFim
                };
                _doctorsScheduleRepository.UpdateDoctorsSchedule(agenda);
                return Ok(new { message = "Agenda atualizada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Excluir agenda cadastrada
        /// </summary>
        /// <param name="id">Identificador da agenda</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public IActionResult DeleteDoctorsSchedule(int id)
        {
            try
            {
                var agendaExistente = _doctorsScheduleRepository.ListDoctorsSchedule().FirstOrDefault(d => d.Id == id);
                if (agendaExistente == null)
                {
                    return NotFound("Agenda não encontrada");
                }
                _doctorsScheduleRepository.DeleteDoctorsSchedule(id);
                return Ok(new { message = "Agenda excluída com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}