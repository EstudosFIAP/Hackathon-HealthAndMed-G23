using HealthAndMed.API.Controllers;
using HealthAndMed.API.Models;
using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace HealthAndMed.Tests.Controllers
{
    public class AppointmentControllerTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IDoctorsScheduleRepository> _doctorsScheduleRepositoryMock;
        private readonly AppointmentController _controller;

        public AppointmentControllerTests()
        {
            _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            _doctorRepositoryMock = new Mock<IDoctorRepository>();
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _doctorsScheduleRepositoryMock = new Mock<IDoctorsScheduleRepository>();

            _controller = new AppointmentController(
                _appointmentRepositoryMock.Object,
                _doctorRepositoryMock.Object,
                _patientRepositoryMock.Object,
                _doctorsScheduleRepositoryMock.Object
            );
        }

        #region GET

        [Fact]
        public void GetAppointment_ReturnsAppointments_ForDoctor()
        {
            // Arrange - simula usuário com role "Médico"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "100"),
                new Claim(ClaimTypes.Role, "Médico")
            };

            var identity = new ClaimsIdentity(claims, "BasicAuthentication");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var appointments = new List<AppointmentDTO>
            {
                new AppointmentDTO
                {
                    Id = 1,
                    IdPaciente = 10,
                    IdMedico = 20,
                    IdAgenda = 30,
                    StatusAgendamento = "Pendente",
                    DataCriacao = DateTime.Now,
                    DataModificacao = DateTime.Now
                },
                new AppointmentDTO
                {
                    Id = 2,
                    IdPaciente = 11,
                    IdMedico = 21,
                    IdAgenda = 31,
                    StatusAgendamento = "Agendado",
                    DataCriacao = DateTime.Now,
                    DataModificacao = DateTime.Now
                }
            };
            _appointmentRepositoryMock.Setup(r => r.ListAppointments()).Returns(appointments);

            // Configura o repositório de médicos para retornar um médico que corresponda ao usuário autenticado
            var doctors = new List<Medico>
            {
                new Medico { Id = 20, IdUsuario = 100 },
                new Medico { Id = 22, IdUsuario = 200 }
            };

            _doctorRepositoryMock.Setup(r => r.GetAllDoctors()).Returns(doctors);

            // Act
            var result = _controller.GetAppointment();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointments = Assert.IsAssignableFrom<IEnumerable<AppointmentDTO>>(okResult.Value);

            Assert.Single(returnedAppointments);
            Assert.Equal(1, returnedAppointments.First().Id);
        }

        [Fact]
        public void GetAppointment_ReturnsAppointments_ForPatient()
        {
            // Arrange - simula usuário com role "Paciente"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "200"),
                new Claim(ClaimTypes.Role, "Paciente")
            };
            var identity = new ClaimsIdentity(claims, "BasicAuthentication");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var appointments = new List<AppointmentDTO>
            {
                new AppointmentDTO
                {
                    Id = 1,
                    IdPaciente = 5,
                    IdMedico = 20,
                    IdAgenda = 30,
                    StatusAgendamento = "Pendente",
                    DataCriacao = DateTime.Now,
                    DataModificacao = DateTime.Now
                },
                new AppointmentDTO
                {
                    Id = 2,
                    IdPaciente = 6,
                    IdMedico = 21,
                    IdAgenda = 31,
                    StatusAgendamento = "Agendado",
                    DataCriacao = DateTime.Now,
                    DataModificacao = DateTime.Now
                }
            };
            _appointmentRepositoryMock.Setup(r => r.ListAppointments()).Returns(appointments);

            var patient = new Paciente { Id = 5, IdUsuario = 200 };
            _patientRepositoryMock.Setup(r => r.GetPatientByUserId(200)).Returns(patient);

            // Act
            var result = _controller.GetAppointment();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointments = Assert.IsAssignableFrom<IEnumerable<AppointmentDTO>>(okResult.Value);
            Assert.Single(returnedAppointments);
            Assert.Equal(1, returnedAppointments.First().Id);
        }

        [Fact]
        public void GetAppointment_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Throws(new Exception("Test exception"));

            // Act
            var result = _controller.GetAppointment();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion

        #region POST

        public void CreateAppointment_ReturnsOk_ForValidAppointment()
        {
            // Arrange
            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20
            };

            var scheduleDto = new DoctorsScheduleDTO
            {
                Id = 30,
                IsBloqueado = false,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { scheduleDto });

            var createdAppointment = new Agendamento
            {
                Id = 1,
                IdPaciente = inputAppointment.IdPaciente,
                IdAgenda = inputAppointment.IdAgenda,
                IdMedico = inputAppointment.IdMedico,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now,
                StatusAgendamento = "Pendente"
            };
            _appointmentRepositoryMock.Setup(r => r.CreateAppointment(It.IsAny<Agendamento>()))
                .Returns(createdAppointment);

            // Act
            var result = _controller.CreateAppointment(inputAppointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Agendamento criado com sucesso!", messageValue);

            _doctorsScheduleRepositoryMock.Verify(r => r.UpdateDoctorsSchedule(
                It.Is<Agenda>(a => a.Id == scheduleDto.Id && a.IsBloqueado == true)
            ), Times.Once);
        }

        [Fact]
        public void CreateAppointment_ReturnsNotFound_WhenAgendaNotFound()
        {
            // Arrange
            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 10,
                IdAgenda = 999,
                IdMedico = 20
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO>());

            // Act
            var result = _controller.CreateAppointment(inputAppointment);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Agenda não encontrada", notFoundResult.Value);
        }

        [Fact]
        public void CreateAppointment_ReturnsBadRequest_WhenAgendaAlreadyBlocked()
        {
            // Arrange
            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20
            };

            var scheduleDto = new DoctorsScheduleDTO
            {
                Id = 30,
                IsBloqueado = true,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { scheduleDto });

            // Act
            var result = _controller.CreateAppointment(inputAppointment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("A agenda selecionada já está reservada e não poderá ser utilizada, por favor, selecione outra opção e tente novamente.", badRequestResult.Value);
        }

        [Fact]
        public void CreateAppointment_ReturnsBadRequest_OnException()
        {
            // Arrange
            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20
            };

            var scheduleDto = new DoctorsScheduleDTO
            {
                Id = 30,
                IsBloqueado = false,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { scheduleDto });

            _appointmentRepositoryMock.Setup(r => r.CreateAppointment(It.IsAny<Agendamento>()))
                .Throws(new Exception("Creation error"));

            // Act
            var result = _controller.CreateAppointment(inputAppointment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Creation error", badRequestResult.Value);
        }

        #endregion

        #region PUT (Update Appointment)

        [Fact]
        public void UpdateAppointment_ReturnsOk_WhenAppointmentExists()
        {
            // Arrange
            int appointmentId = 1;
            var existingAppointmentDTO = new AppointmentDTO
            {
                Id = appointmentId,
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20,
                StatusAgendamento = "Pendente",
                MotivoCancelamento = "",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { existingAppointmentDTO });

            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 11,
                IdAgenda = 31,
                IdMedico = 21
            };

            // Act
            var result = _controller.UpdateAppointment(appointmentId, inputAppointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Agendamento atualizado com sucesso!", messageValue);

            _appointmentRepositoryMock.Verify(r => r.UpdateAppointment(
                It.Is<Agendamento>(a =>
                    a.Id == appointmentId &&
                    a.IdPaciente == inputAppointment.IdPaciente &&
                    a.IdAgenda == inputAppointment.IdAgenda &&
                    a.IdMedico == inputAppointment.IdMedico)
            ), Times.Once);
        }

        [Fact]
        public void UpdateAppointment_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            // Arrange
            int appointmentId = 1;
            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO>());

            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 11,
                IdAgenda = 31,
                IdMedico = 21
            };

            // Act
            var result = _controller.UpdateAppointment(appointmentId, inputAppointment);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Agendamento não encontrado", notFoundResult.Value);
        }

        [Fact]
        public void UpdateAppointment_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int appointmentId = 1;
            var existingAppointmentDTO = new AppointmentDTO
            {
                Id = appointmentId,
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20,
                StatusAgendamento = "Pendente",
                MotivoCancelamento = "",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { existingAppointmentDTO });

            var inputAppointment = new AppointmentModel
            {
                IdPaciente = 11,
                IdAgenda = 31,
                IdMedico = 21
            };

            _appointmentRepositoryMock.Setup(r => r.UpdateAppointment(It.IsAny<Agendamento>()))
                .Throws(new Exception("Update error"));

            // Act
            var result = _controller.UpdateAppointment(appointmentId, inputAppointment);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion

        #region PUT (Cancel Appointment)

        [Fact]
        public void CancelAppointment_ReturnsOk_ForValidCancellation_AsPatient()
        {
            // Arrange - simula usuário com role "Paciente"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "300"),
                new Claim(ClaimTypes.Role, "Paciente")
            };

            var identity = new ClaimsIdentity(claims, "BasicAuthentication");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Agendamento com status "Pendente"
            var appointment = new AppointmentDTO
            {
                Id = 1,
                IdPaciente = 300,
                IdMedico = 20,
                IdAgenda = 40,
                StatusAgendamento = "Pendente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { appointment });

            var patient = new Paciente { Id = 300, IdUsuario = 300 };
            _patientRepositoryMock.Setup(r => r.GetPatientByUserId(300)).Returns(patient);

            // Act
            string justificativa = "Não poderei comparecer";
            var result = _controller.CancelAppointment(1, justificativa);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Consulta cancelada com sucesso!", messageValue);

            _appointmentRepositoryMock.Verify(r => r.UpdateAppointment(
                It.Is<Agendamento>(a =>
                    a.Id == 1 &&
                    a.StatusAgendamento == "Cancelado" &&
                    a.MotivoCancelamento == justificativa)
            ), Times.Once);

            _doctorsScheduleRepositoryMock.Verify(r => r.UpdateDoctorsSchedule(
                It.Is<Agenda>(a => a.Id == appointment.IdAgenda && a.IsBloqueado == false)
            ), Times.Once);
        }

        [Fact]
        public void CancelAppointment_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            // Arrange - simula usuário com role "Paciente"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "300"),
                new Claim(ClaimTypes.Role, "Paciente")
            };
            var identity = new ClaimsIdentity(claims, "BasicAuthentication");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO>());

            // Act
            var result = _controller.CancelAppointment(1, "Justificativa");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Consulta não encontrada ou não pertence ao paciente autenticado.", notFoundResult.Value);
        }

        [Fact]
        public void CancelAppointment_ReturnsBadRequest_WhenStatusNotCancelable()
        {
            // Arrange - simula usuário com role "Paciente"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "300"),
                new Claim(ClaimTypes.Role, "Paciente")
            };

            var identity = new ClaimsIdentity(claims, "BasicAuthentication");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var appointment = new AppointmentDTO
            {
                Id = 2,
                IdPaciente = 300,
                IdMedico = 20,
                IdAgenda = 40,
                StatusAgendamento = "Recusado",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { appointment });

            var patient = new Paciente { Id = 300, IdUsuario = 300 };
            _patientRepositoryMock.Setup(r => r.GetPatientByUserId(300)).Returns(patient);

            // Act
            var result = _controller.CancelAppointment(2, "Justificativa");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("A consulta não pode ser cancelada, pois não está no status 'Pendente' ou 'Agendado'.", badRequestResult.Value);
        }

        #endregion

        #region PUT (Update Appointment Status)

        [Fact]
        public void UpdateAppointmentStatus_ReturnsOk_ForAcceptedStatus_AsDoctor()
        {
            // Arrange - simula usuário com role "Médico"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "400"),
                new Claim(ClaimTypes.Role, "Médico")
            };
            var identity = new ClaimsIdentity(claims, "BasicAuthentication");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var appointment = new AppointmentDTO
            {
                Id = 3,
                IdPaciente = 50,
                IdMedico = 60,
                IdAgenda = 70,
                StatusAgendamento = "Pendente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { appointment });

            var doctors = new List<Medico>
            {
                new Medico { Id = 60, IdUsuario = 400 }
            };
            _doctorRepositoryMock.Setup(r => r.GetAllDoctors()).Returns(doctors);

            // Act
            var result = _controller.UpdateAppointmentStatus(3, "Aceito");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Status da consulta alterado para Aceito com sucesso!", messageValue);

            _appointmentRepositoryMock.Verify(r => r.UpdateAppointment(
                It.Is<Agendamento>(a => a.Id == 3 && a.StatusAgendamento == "Agendado")
            ), Times.Once);
        }

        [Fact]
        public void UpdateAppointmentStatus_ReturnsBadRequest_ForInvalidStatus()
        {
            // Arrange - simula usuário com role "Administrador"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "500"),
                new Claim(ClaimTypes.Role, "Administrador")
            };
            var identity = new ClaimsIdentity(claims, "BasicAuthentication");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Agendamento com status "Pendente"
            var appointment = new AppointmentDTO
            {
                Id = 4,
                IdPaciente = 55,
                IdMedico = 65,
                IdAgenda = 75,
                StatusAgendamento = "Pendente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { appointment });

            // Act - envia status inválido
            var result = _controller.UpdateAppointmentStatus(4, "Invalido");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Status inválido. Use 'Aceito' ou 'Recusado'.", badRequestResult.Value);
        }

        #endregion

        #region DELETE

        [Fact]
        public void DeleteAppointment_ReturnsOk_WhenAppointmentExists()
        {
            // Arrange
            int appointmentId = 1;
            var existingAppointmentDTO = new AppointmentDTO
            {
                Id = appointmentId,
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20,
                StatusAgendamento = "Agendado",
                MotivoCancelamento = "",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { existingAppointmentDTO });

            // Act
            var result = _controller.DeleteAppointment(appointmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Agendamento excluído com sucesso!", messageValue);

            _appointmentRepositoryMock.Verify(r => r.DeleteAppointment(appointmentId), Times.Once);
        }

        [Fact]
        public void DeleteAppointment_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            // Arrange
            int appointmentId = 1;
            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO>());

            // Act
            var result = _controller.DeleteAppointment(appointmentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Agendamento não encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DeleteAppointment_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int appointmentId = 1;
            var existingAppointmentDTO = new AppointmentDTO
            {
                Id = appointmentId,
                IdPaciente = 10,
                IdAgenda = 30,
                IdMedico = 20,
                StatusAgendamento = "Agendado",
                MotivoCancelamento = "",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _appointmentRepositoryMock.Setup(r => r.ListAppointments())
                .Returns(new List<AppointmentDTO> { existingAppointmentDTO });
            _appointmentRepositoryMock.Setup(r => r.DeleteAppointment(appointmentId))
                .Throws(new Exception("Delete error"));

            // Act
            var result = _controller.DeleteAppointment(appointmentId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion
    }
}