using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthAndMed.API.Repository.Interfaces;
using System.Linq;

namespace HealthAndMed.API.Controllers
{
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorsScheduleRepository _doctorsScheduleRepository;

        /// <summary>
        /// Construtor do DoctorController.
        /// </summary>
        /// <param name="doctorRepository">Repositório de médicos.</param>
        /// <param name="doctorsScheduleRepository">Repositório de agendas médicas.</param>
        public DoctorController(IDoctorRepository doctorRepository, IDoctorsScheduleRepository doctorsScheduleRepository)
        {
            _doctorRepository = doctorRepository;
            _doctorsScheduleRepository = doctorsScheduleRepository;
        }

        /// <summary>
        /// Listar medicos com filtros especificos.
        /// </summary>
        /// <param name="nome">Nome do medico.</param>
        /// <param name="especialidade">Especialidade do medico.</param>
        /// <param name="numeroCrm">Numero CRM do medico.</param>
        /// <param name="id">ID do medico.</param>
        /// <returns>Lista de medicos com detalhes do usuario.</returns>
        [Authorize(Roles = "Paciente,Administrador")]
        [HttpGet]
        public IActionResult GetDoctors(
            [FromQuery] string? nome = null,
            [FromQuery] string? especialidade = null,
            [FromQuery] string? numeroCrm = null,
            [FromQuery] int? id = null)
        {
            try
            {
                var medico = _doctorRepository.GetAllDoctors();

                // Aplicando os filtros opcionais
                if (!string.IsNullOrEmpty(nome))
                {
                    medico = medico.Where(d => d.Usuario.Nome != null && d.Usuario.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(especialidade))
                {
                    medico = medico.Where(d => d.Especialidade != null && d.Especialidade.Contains(especialidade, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(numeroCrm))
                {
                    medico = medico.Where(d => d.NumeroCrm != null && d.NumeroCrm.Contains(numeroCrm, StringComparison.OrdinalIgnoreCase));
                }

                if (id.HasValue)
                {
                    medico = medico.Where(d => d.Id == id.Value);
                }

                var result = medico.Select(d => new
                {
                    Id = d.Id,
                    Nome = d.Usuario.Nome,
                    Email = d.Usuario.Email,
                    NumeroCrm = d.NumeroCrm,
                    Especialidade = d.Especialidade,
                    ValorConsulta = d.ValorConsulta,
                    DataCriacao = d.DataCriacao,
                    DataModificacao = d.DataModificacao
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }

        }

        /// <summary>
        /// Lista medicos com agendas disponiveis futuras
        /// </summary>
        /// <param name="nome">Nome do medico.</param>
        /// <param name="especialidade">Especialidade do medico.</param>
        /// <param name="numeroCrm">Numero CRM do medico.</param>
        /// <param name="id">ID do medico.</param>
        /// <returns>Lista de medicos com detalhes das agendas futuras</returns>
        [Authorize(Roles = "Paciente,Administrador")]
        [HttpGet("with-future-schedules")]
        public IActionResult GetDoctorsWithFutureSchedules(
            [FromQuery] string? nome = null,
            [FromQuery] string? especialidade = null,
            [FromQuery] string? numeroCrm = null,
            [FromQuery] int? id = null)
        {
            try
            {
                var medicos = _doctorRepository.GetAllDoctors();
                var futureSchedules = _doctorsScheduleRepository.ListDoctorsSchedule()
                    .Where(schedule => schedule.HorarioInicio > DateTime.Now && !schedule.IsBloqueado);

                if (!string.IsNullOrEmpty(nome))
                {
                    medicos = medicos.Where(d => d.Usuario.Nome != null && d.Usuario.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(especialidade))
                {
                    medicos = medicos.Where(d => d.Especialidade != null && d.Especialidade.Contains(especialidade, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(numeroCrm))
                {
                    medicos = medicos.Where(d => d.NumeroCrm != null && d.NumeroCrm.Contains(numeroCrm, StringComparison.OrdinalIgnoreCase));
                }

                if (id.HasValue)
                {
                    medicos = medicos.Where(d => d.Id == id.Value);
                }

                var result = medicos.Select(d => new
                {
                    Id = d.Id,
                    Nome = d.Usuario.Nome,
                    Email = d.Usuario.Email,
                    NumeroCrm = d.NumeroCrm,
                    Especialidade = d.Especialidade,
                    ValorConsulta = d.ValorConsulta,
                    AgendasFuturas = futureSchedules
                        .Where(schedule => schedule.IdMedico == d.Id)
                        .Select(schedule => new
                        {
                            schedule.Id,
                            schedule.HorarioInicio,
                            schedule.HorarioFim,
                            schedule.DataCriacao,
                            schedule.DataModificacao
                        }).ToList()
                }).Where(d => d.AgendasFuturas.Any()).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
    }
}
