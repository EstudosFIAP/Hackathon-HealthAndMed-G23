using HealthAndMed.API.Repository;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Tests.Integration
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly DbHmContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DbHmContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            _context = new DbHmContext(options);
            _repository = new UserRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CreateUser_Should_AddUser_ToDatabase()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nome = "Test User",
                Cpf = "123456789",
                Email = "test@example.com",
                Senha = "pass",
                TipoUsuario = "paciente"
            };

            // Act
            var createdUser = _repository.CreateUser(usuario);

            // Assert
            Assert.NotEqual(0, createdUser.Id);
            var userInDb = _context.Usuario.Find(createdUser.Id);
            Assert.NotNull(userInDb);
            Assert.Equal("Test User", userInDb.Nome);
        }

        [Fact]
        public void ListUsers_Should_ReturnAddedUsers()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nome = "Test User",
                Cpf = "123456789",
                Email = "test@example.com",
                Senha = "pass",
                TipoUsuario = "paciente"
            };
            _context.Usuario.Add(usuario);
            _context.SaveChanges();

            // Act
            var users = _repository.ListUsers();

            // Assert
            Assert.NotEmpty(users);
            Assert.Single(users);
        }

        [Fact]
        public void UpdateUser_Should_ModifyExistingUser()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nome = "Test User",
                Cpf = "123456789",
                Email = "test@example.com",
                Senha = "pass",
                TipoUsuario = "paciente"
            };
            _context.Usuario.Add(usuario);
            _context.SaveChanges();

            var updatedUser = new Usuario
            {
                Id = usuario.Id,
                Nome = "Updated User",
                Cpf = "987654321",
                Email = "updated@example.com",
                Senha = "newpass",
                TipoUsuario = "paciente"
            };

            // Act
            _repository.UpdateUser(updatedUser);

            // Assert
            var userInDb = _context.Usuario.Find(usuario.Id);
            Assert.Equal("Updated User", userInDb.Nome);
            Assert.Equal("987654321", userInDb.Cpf);
            Assert.Equal("updated@example.com", userInDb.Email);
            Assert.Equal("newpass", userInDb.Senha);
        }

        [Fact]
        public void DeleteUser_Should_RemoveUser_FromDatabase()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nome = "Test User",
                Cpf = "123456789",
                Email = "test@example.com",
                Senha = "pass",
                TipoUsuario = "paciente"
            };
            _context.Usuario.Add(usuario);
            _context.SaveChanges();

            // Act
            _repository.DeleteUser(usuario.Id);

            // Assert
            var userInDb = _context.Usuario.Find(usuario.Id);
            Assert.Null(userInDb);
        }

        [Fact]
        public void DeleteUser_Should_ThrowException_WhenUserNotFound()
        {
            // Arrange
            int invalidId = 999;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => _repository.DeleteUser(invalidId));
        }
    }
}
