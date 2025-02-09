using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthAndMed.API.Models;
using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.Entities;
using System.Numerics;

namespace HealthAndMed.API.Controllers
{
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        /// <summary>
        /// Construtor do UserController.
        /// </summary>
        /// <param name="userRepository">Repositório de usuários.</param>
        /// <param name="doctorRepository">Repositório de médicos.</param>
        /// <param name="patientRepository">Repositório de pacientes.</param>
        public UserController(
            IUserRepository userRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository)
        {
            _userRepository = userRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// Listar todos os usuários.
        /// </summary>
        /// <returns>Uma lista de usuários.</returns>
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult GetUser()
        {
            try
            {
                var result = _userRepository.ListUsers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Criar usuário.
        /// </summary>
        /// <param name="inputUser">Modelo com os dados do usuário.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public IActionResult CreateUser([FromBody] UserModel inputUser)
        {
            try
            {
                var usuario = new Usuario
                {
                    Nome = inputUser.Nome,
                    Cpf = inputUser.Cpf,
                    Senha = inputUser.Senha,
                    Email = inputUser.Email,
                    TipoUsuario = inputUser.TipoUsuario,
                    Data_Criacao = DateTime.Now,
                    Data_Modificacao = DateTime.Now
                };

                var usuarioCriado = _userRepository.CreateUser(usuario);

                if (inputUser.TipoUsuario?.ToLower() == "médico")
                {
                    if (string.IsNullOrEmpty(inputUser.NumeroCrm) ||
                        string.IsNullOrEmpty(inputUser.Especialidade) ||
                        inputUser.ValorConsulta == null)
                    {
                        return BadRequest("Campos adicionais (NumeroCrm, Especialidade e ValorConsulta) são obrigatórios para médicos.");
                    }

                    var medico = new Medico
                    {
                        IdUsuario = usuarioCriado.Id,
                        NumeroCrm = inputUser.NumeroCrm,
                        Especialidade = inputUser.Especialidade,
                        ValorConsulta = inputUser.ValorConsulta,
                        DataCriacao = DateTime.Now,
                        DataModificacao = DateTime.Now
                    };

                    _doctorRepository.CreateDoctor(medico);
                }else if (inputUser.TipoUsuario?.ToLower() == "paciente")
                {
                    var paciente = new Paciente
                    {
                        IdUsuario = usuarioCriado.Id,
                        DataCriacao = DateTime.Now,
                        DataModificacao = DateTime.Now
                    };
                    _patientRepository.CreatePatient(paciente);
                }

                return Ok(new { message = "Usuário criado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualizar usuário cadastrado.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <param name="inputUser">Modelo com os dados atualizados do usuário.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserModel inputUser)
        {
            try
            {
                var usuarioExistente = _userRepository.ListUsers().FirstOrDefault(u => u.Id == id);
                if (usuarioExistente == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var usuario = new Usuario
                {
                    Id = usuarioExistente.Id,
                    Nome = inputUser.Nome,
                    Cpf = inputUser.Cpf,
                    Senha = inputUser.Senha,
                    Email = inputUser.Email,
                    TipoUsuario = inputUser.TipoUsuario
                };

                _userRepository.UpdateUser(usuario);
                return Ok(new { message = "Usuário atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Excluir usuário cadastrado.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var usuarioExistente = _userRepository.ListUsers().FirstOrDefault(u => u.Id == id);
                if (usuarioExistente == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                _userRepository.DeleteUser(id);
                return Ok(new { message = "Usuário excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
