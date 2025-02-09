using HealthAndMed.API.Controllers;
using HealthAndMed.API.Models;
using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HealthAndMed.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _doctorRepositoryMock = new Mock<IDoctorRepository>();
            _patientRepositoryMock = new Mock<IPatientRepository>();

            _controller = new UserController(
                _userRepositoryMock.Object,
                _doctorRepositoryMock.Object,
                _patientRepositoryMock.Object
            );
        }

        [Fact]
        public void GetUser_ReturnsOkResult_WithUserList()
        {
            // Arrange
            var userList = new List<UserDTO>
            {
                new UserDTO
                {
                    Id = 1,
                    Nome = "John",
                    Cpf = "123",
                    Email = "john@example.com",
                    Senha = "password",
                    TipoUsuario = "paciente",
                    DataCriacao = DateTime.Now,
                    DataModificacao = DateTime.Now
                },
                new UserDTO
                {
                    Id = 2,
                    Nome = "Jane",
                    Cpf = "456",
                    Email = "jane@example.com",
                    Senha = "password",
                    TipoUsuario = "médico",
                    DataCriacao = DateTime.Now,
                    DataModificacao = DateTime.Now
                }
            };

            _userRepositoryMock.Setup(r => r.ListUsers()).Returns(userList);

            // Act
            var result = _controller.GetUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact]
        public void GetUser_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.ListUsers()).Throws(new Exception("Test exception"));

            // Act
            var result = _controller.GetUser();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public void CreateUser_ReturnsOk_ForPaciente()
        {
            // Arrange
            var inputUser = new UserModel
            {
                Nome = "Paciente1",
                Cpf = "111",
                Senha = "pass",
                Email = "paciente1@example.com",
                TipoUsuario = "paciente"
            };

            var createdUser = new Usuario
            {
                Id = 1,
                Nome = inputUser.Nome,
                Cpf = inputUser.Cpf,
                Senha = inputUser.Senha,
                Email = inputUser.Email,
                TipoUsuario = inputUser.TipoUsuario,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now
            };

            _userRepositoryMock.Setup(r => r.CreateUser(It.IsAny<Usuario>())).Returns(createdUser);

            // Act
            var result = _controller.CreateUser(inputUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(value, null);

            Assert.Equal("Usuário criado com sucesso!", messageValue);

            _patientRepositoryMock.Verify(r => r.CreatePatient(
                It.Is<Paciente>(p => p.IdUsuario == createdUser.Id)), Times.Once);
            _doctorRepositoryMock.Verify(r => r.CreateDoctor(It.IsAny<Medico>()), Times.Never);
        }

        [Fact]
        public void CreateUser_ReturnsOk_ForMedico_WithValidData()
        {
            // Arrange
            var inputUser = new UserModel
            {
                Nome = "Medico1",
                Cpf = "222",
                Senha = "pass",
                Email = "medico1@example.com",
                TipoUsuario = "médico",
                NumeroCrm = "CRM123",
                Especialidade = "Cardiologia",
                ValorConsulta = 150.00m
            };

            var createdUser = new Usuario
            {
                Id = 2,
                Nome = inputUser.Nome,
                Cpf = inputUser.Cpf,
                Senha = inputUser.Senha,
                Email = inputUser.Email,
                TipoUsuario = inputUser.TipoUsuario,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now
            };

            _userRepositoryMock.Setup(r => r.CreateUser(It.IsAny<Usuario>())).Returns(createdUser);

            // Act
            var result = _controller.CreateUser(inputUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty); 
            var messageValue = messageProperty.GetValue(value, null);

            Assert.Equal("Usuário criado com sucesso!", messageValue);

            _doctorRepositoryMock.Verify(r => r.CreateDoctor(
                It.Is<Medico>(m => m.IdUsuario == createdUser.Id &&
                                    m.NumeroCrm == inputUser.NumeroCrm &&
                                    m.Especialidade == inputUser.Especialidade &&
                                    m.ValorConsulta == inputUser.ValorConsulta)), Times.Once);
            _patientRepositoryMock.Verify(r => r.CreatePatient(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public void CreateUser_ReturnsBadRequest_ForMedico_WithMissingData()
        {
            // Arrange
            var inputUser = new UserModel
            {
                Nome = "Medico1",
                Cpf = "222",
                Senha = "pass",
                Email = "medico1@example.com",
                TipoUsuario = "médico"
            };

            // Act
            var result = _controller.CreateUser(inputUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Campos adicionais (NumeroCrm, Especialidade e ValorConsulta) são obrigatórios para médicos.", badRequestResult.Value);
        }

        [Fact]
        public void CreateUser_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var inputUser = new UserModel
            {
                Nome = "Paciente1",
                Cpf = "111",
                Senha = "pass",
                Email = "paciente1@example.com",
                TipoUsuario = "paciente"
            };

            _userRepositoryMock.Setup(r => r.CreateUser(It.IsAny<Usuario>()))
                               .Throws(new Exception("Database error"));

            // Act
            var result = _controller.CreateUser(inputUser);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public void UpdateUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            int userId = 1;
            var existingUserDTO = new UserDTO
            {
                Id = userId,
                Nome = "Existing",
                Cpf = "123",
                Email = "existing@example.com",
                Senha = "pass",
                TipoUsuario = "paciente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _userRepositoryMock.Setup(r => r.ListUsers())
                               .Returns(new List<UserDTO> { existingUserDTO });

            var inputUser = new UserModel
            {
                Nome = "Updated",
                Cpf = "1234",
                Senha = "newpass",
                Email = "updated@example.com",
                TipoUsuario = "paciente"
            };

            // Act
            var result = _controller.UpdateUser(userId, inputUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            var messageValue = messageProperty.GetValue(value, null);

            Assert.Equal("Usuário atualizado com sucesso!", messageValue);

            _userRepositoryMock.Verify(r => r.UpdateUser(
                It.Is<Usuario>(u => u.Id == userId &&
                                    u.Nome == inputUser.Nome &&
                                    u.Cpf == inputUser.Cpf &&
                                    u.Email == inputUser.Email &&
                                    u.Senha == inputUser.Senha &&
                                    u.TipoUsuario == inputUser.TipoUsuario)
            ), Times.Once);
        }

        [Fact]
        public void UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 1;
            _userRepositoryMock.Setup(r => r.ListUsers()).Returns(new List<UserDTO>());

            var inputUser = new UserModel
            {
                Nome = "Updated",
                Cpf = "1234",
                Senha = "newpass",
                Email = "updated@example.com",
                TipoUsuario = "paciente"
            };

            // Act
            var result = _controller.UpdateUser(userId, inputUser);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Usuário não encontrado", notFoundResult.Value);
        }

        [Fact]
        public void UpdateUser_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int userId = 1;
            var existingUserDTO = new UserDTO
            {
                Id = userId,
                Nome = "Existing",
                Cpf = "123",
                Email = "existing@example.com",
                Senha = "pass",
                TipoUsuario = "paciente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };
            _userRepositoryMock.Setup(r => r.ListUsers())
                               .Returns(new List<UserDTO> { existingUserDTO });

            var inputUser = new UserModel
            {
                Nome = "Updated",
                Cpf = "1234",
                Senha = "newpass",
                Email = "updated@example.com",
                TipoUsuario = "paciente"
            };

            _userRepositoryMock.Setup(r => r.UpdateUser(It.IsAny<Usuario>()))
                               .Throws(new Exception("Update error"));

            // Act
            var result = _controller.UpdateUser(userId, inputUser);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public void DeleteUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            int userId = 1;
            var existingUserDTO = new UserDTO
            {
                Id = userId,
                Nome = "Existing",
                Cpf = "123",
                Email = "existing@example.com",
                Senha = "pass",
                TipoUsuario = "paciente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };

            _userRepositoryMock.Setup(r => r.ListUsers())
                               .Returns(new List<UserDTO> { existingUserDTO });

            // Act
            var result = _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(value, null);

            Assert.Equal("Usuário excluído com sucesso!", messageValue);

            _userRepositoryMock.Verify(r => r.DeleteUser(userId), Times.Once);
        }

        [Fact]
        public void DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 1;
            _userRepositoryMock.Setup(r => r.ListUsers()).Returns(new List<UserDTO>());

            // Act
            var result = _controller.DeleteUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Usuário não encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DeleteUser_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int userId = 1;
            var existingUserDTO = new UserDTO
            {
                Id = userId,
                Nome = "Existing",
                Cpf = "123",
                Email = "existing@example.com",
                Senha = "pass",
                TipoUsuario = "paciente",
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };
            _userRepositoryMock.Setup(r => r.ListUsers())
                               .Returns(new List<UserDTO> { existingUserDTO });
            _userRepositoryMock.Setup(r => r.DeleteUser(userId))
                               .Throws(new Exception("Delete error"));

            // Act
            var result = _controller.DeleteUser(userId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
