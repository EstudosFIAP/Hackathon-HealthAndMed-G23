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
    public class DoctorsScheduleControllerTests
    {
        private readonly Mock<IDoctorsScheduleRepository> _doctorsScheduleRepositoryMock;
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
        private readonly DoctorsScheduleController _controller;

        public DoctorsScheduleControllerTests()
        {
            _doctorsScheduleRepositoryMock = new Mock<IDoctorsScheduleRepository>();
            _doctorRepositoryMock = new Mock<IDoctorRepository>();
            _controller = new DoctorsScheduleController(_doctorsScheduleRepositoryMock.Object, _doctorRepositoryMock.Object);
        }

        #region GET

        [Fact]
        public void GetDoctorsSchedule_ReturnsSchedules_ForDoctorRole()
        {
            // Simula usuário com role "Médico"
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

            var schedules = new List<DoctorsScheduleDTO>
            {
                new DoctorsScheduleDTO
                {
                    Id = 1,
                    IdMedico = 10,
                    IsBloqueado = false,
                    HorarioInicio = DateTime.Now.AddHours(9),
                    HorarioFim = DateTime.Now.AddHours(11)
                },
                new DoctorsScheduleDTO
                {
                    Id = 2,
                    IdMedico = 20,  // agenda de outro médico
                    IsBloqueado = false,
                    HorarioInicio = DateTime.Now.AddHours(13),
                    HorarioFim = DateTime.Now.AddHours(15)
                }
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule()).Returns(schedules);

            var doctors = new List<Medico>
            {
                new Medico { Id = 10, IdUsuario = 100 },
                new Medico { Id = 30, IdUsuario = 300 }
            };

            _doctorRepositoryMock.Setup(r => r.GetAllDoctors()).Returns(doctors);

            // Act
            var result = _controller.GetDoctorsSchedule();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSchedules = Assert.IsAssignableFrom<IEnumerable<DoctorsScheduleDTO>>(okResult.Value);

            Assert.Single(returnedSchedules);
            Assert.Equal(1, returnedSchedules.First().Id);
        }

        [Fact]
        public void GetDoctorsSchedule_ReturnsAllSchedules_ForAdministratorRole()
        {
            // Simula usuário com role "Administrador"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Role, "Administrador")
            };
            var identity = new ClaimsIdentity(claims, "BasicAuthentication");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var schedules = new List<DoctorsScheduleDTO>
            {
                new DoctorsScheduleDTO
                {
                    Id = 1,
                    IdMedico = 10,
                    IsBloqueado = false,
                    HorarioInicio = DateTime.Now.AddHours(9),
                    HorarioFim = DateTime.Now.AddHours(11)
                },
                new DoctorsScheduleDTO
                {
                    Id = 2,
                    IdMedico = 20,
                    IsBloqueado = false,
                    HorarioInicio = DateTime.Now.AddHours(13),
                    HorarioFim = DateTime.Now.AddHours(15)
                }
            };
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule()).Returns(schedules);

            // Act
            var result = _controller.GetDoctorsSchedule();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSchedules = Assert.IsAssignableFrom<IEnumerable<DoctorsScheduleDTO>>(okResult.Value);
            Assert.Equal(2, returnedSchedules.Count());
        }

        [Fact]
        public void GetDoctorsSchedule_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule()).Throws(new Exception("Test exception"));

            // Act
            var result = _controller.GetDoctorsSchedule();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion

        #region POST

        [Fact]
        public void CreateDoctorsSchedule_ReturnsOk_ForValidSchedule()
        {
            // Arrange
            var inputModel = new DoctorsScheduleModel
            {
                IdMedico = 10,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };

            // Simula a criação da agenda no repositório
            var createdAgenda = new Agenda
            {
                Id = 1,
                IdMedico = inputModel.IdMedico,
                HorarioInicio = inputModel.HorarioInicio,
                HorarioFim = inputModel.HorarioFim,
                IsBloqueado = false,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.CreateDoctorsSchedule(It.IsAny<Agenda>()))
                .Returns(createdAgenda);

            // Act
            var result = _controller.CreateDoctorsSchedule(inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Agenda criada com sucesso!", messageValue);
        }

        [Fact]
        public void CreateDoctorsSchedule_ReturnsBadRequest_OnException()
        {
            // Arrange
            var inputModel = new DoctorsScheduleModel
            {
                IdMedico = 10,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.CreateDoctorsSchedule(It.IsAny<Agenda>()))
                .Throws(new Exception("Creation error"));

            // Act
            var result = _controller.CreateDoctorsSchedule(inputModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Creation error", badRequestResult.Value);
        }

        #endregion

        #region PUT (Update)

        [Fact]
        public void UpdateDoctorsSchedule_ReturnsOk_WhenScheduleExists()
        {
            // Arrange
            int scheduleId = 1;
            var existingScheduleDTO = new DoctorsScheduleDTO
            {
                Id = scheduleId,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { existingScheduleDTO });

            var inputModel = new DoctorsScheduleModel
            {
                HorarioInicio = DateTime.Now.AddHours(10),
                HorarioFim = DateTime.Now.AddHours(12)
            };

            // Act
            var result = _controller.UpdateDoctorsSchedule(scheduleId, inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Agenda atualizada com sucesso!", messageValue);

            _doctorsScheduleRepositoryMock.Verify(r => r.UpdateDoctorsSchedule(
                It.Is<Agenda>(a =>
                    a.Id == scheduleId &&
                    a.HorarioInicio == inputModel.HorarioInicio &&
                    a.HorarioFim == inputModel.HorarioFim)
            ), Times.Once);
        }

        [Fact]
        public void UpdateDoctorsSchedule_ReturnsNotFound_WhenScheduleDoesNotExist()
        {
            // Arrange
            int scheduleId = 1;
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO>());

            var inputModel = new DoctorsScheduleModel
            {
                HorarioInicio = DateTime.Now.AddHours(10),
                HorarioFim = DateTime.Now.AddHours(12)
            };

            // Act
            var result = _controller.UpdateDoctorsSchedule(scheduleId, inputModel);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Agenda não encontrada", notFoundResult.Value);
        }

        [Fact]
        public void UpdateDoctorsSchedule_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int scheduleId = 1;
            var existingScheduleDTO = new DoctorsScheduleDTO
            {
                Id = scheduleId,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { existingScheduleDTO });

            var inputModel = new DoctorsScheduleModel
            {
                HorarioInicio = DateTime.Now.AddHours(10),
                HorarioFim = DateTime.Now.AddHours(12)
            };

            _doctorsScheduleRepositoryMock.Setup(r => r.UpdateDoctorsSchedule(It.IsAny<Agenda>()))
                .Throws(new Exception("Update error"));

            // Act
            var result = _controller.UpdateDoctorsSchedule(scheduleId, inputModel);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion

        #region DELETE

        [Fact]
        public void DeleteDoctorsSchedule_ReturnsOk_WhenScheduleExists()
        {
            // Arrange
            int scheduleId = 1;
            var existingScheduleDTO = new DoctorsScheduleDTO
            {
                Id = scheduleId,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { existingScheduleDTO });

            // Act
            var result = _controller.DeleteDoctorsSchedule(scheduleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(value, null);
            Assert.Equal("Agenda excluída com sucesso!", messageValue);

            _doctorsScheduleRepositoryMock.Verify(r => r.DeleteDoctorsSchedule(scheduleId), Times.Once);
        }

        [Fact]
        public void DeleteDoctorsSchedule_ReturnsNotFound_WhenScheduleDoesNotExist()
        {
            // Arrange
            int scheduleId = 1;
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO>());

            // Act
            var result = _controller.DeleteDoctorsSchedule(scheduleId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Agenda não encontrada", notFoundResult.Value);
        }

        [Fact]
        public void DeleteDoctorsSchedule_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int scheduleId = 1;
            var existingScheduleDTO = new DoctorsScheduleDTO
            {
                Id = scheduleId,
                HorarioInicio = DateTime.Now.AddHours(9),
                HorarioFim = DateTime.Now.AddHours(11)
            };
            _doctorsScheduleRepositoryMock.Setup(r => r.ListDoctorsSchedule())
                .Returns(new List<DoctorsScheduleDTO> { existingScheduleDTO });
            _doctorsScheduleRepositoryMock.Setup(r => r.DeleteDoctorsSchedule(scheduleId))
                .Throws(new Exception("Delete error"));

            // Act
            var result = _controller.DeleteDoctorsSchedule(scheduleId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion
    }
}
