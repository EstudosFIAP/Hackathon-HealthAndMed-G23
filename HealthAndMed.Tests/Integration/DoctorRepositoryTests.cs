using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Tests.Integration
{
    public class DoctorRepositoryTests : IDisposable
    {
        private readonly DbHmContext _context;
        private readonly DoctorRepository _repository;

        public DoctorRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DbHmContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Doctor_" + Guid.NewGuid())
                .Options;
            _context = new DbHmContext(options);
            _repository = new DoctorRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CreateDoctor_Should_AddDoctor_ToDatabase()
        {
            // Arrange
            var doctor = new Medico
            {
                IdUsuario = 100,
                NumeroCrm = "CRM123",
                Especialidade = "Cardiologia",
                ValorConsulta = 150.00m,
            };

            // Act
            var createdDoctor = _repository.CreateDoctor(doctor);

            // Assert
            Assert.NotEqual(0, createdDoctor.Id);
            var doctorInDb = _context.Medico.Find(createdDoctor.Id);
            Assert.NotNull(doctorInDb);
            Assert.Equal(100, doctorInDb.IdUsuario);
            Assert.Equal("CRM123", doctorInDb.NumeroCrm);
            Assert.Equal("Cardiologia", doctorInDb.Especialidade);
            Assert.Equal(150.00m, doctorInDb.ValorConsulta);
            Assert.True(doctorInDb.DataCriacao != default(DateTime));
            Assert.True(doctorInDb.DataModificacao != default(DateTime));
        }

        [Fact]
        public void CreateDoctor_Should_ThrowArgumentException_WhenDoctorIsNull()
        {
            // Arrange
            Medico doctor = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _repository.CreateDoctor(doctor));
        }

        [Fact]
        public void GetDoctorByUserId_Should_ReturnDoctor_WhenExists()
        {
            // Arrange
            var doctor = new Medico
            {
                IdUsuario = 200,
                NumeroCrm = "CRM456",
                Especialidade = "Dermatologia",
                ValorConsulta = 200.00m,
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };
            _context.Medico.Add(doctor);
            _context.SaveChanges();

            // Act
            var retrievedDoctor = _repository.GetDoctorByUserId(200);

            // Assert
            Assert.NotNull(retrievedDoctor);
            Assert.Equal("CRM456", retrievedDoctor.NumeroCrm);
        }

        [Fact]
        public void GetDoctorByUserId_Should_ReturnNull_WhenDoctorDoesNotExist()
        {
            // Act
            var retrievedDoctor = _repository.GetDoctorByUserId(9999);

            // Assert
            Assert.Null(retrievedDoctor);
        }
    }
}
