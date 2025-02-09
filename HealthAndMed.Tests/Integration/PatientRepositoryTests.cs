using HealthAndMed.API.Repository;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Tests.Integration
{
    public class PatientRepositoryTests : IDisposable
    {
        private readonly DbHmContext _context;
        private readonly PatientRepository _repository;

        public PatientRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DbHmContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Patient_" + Guid.NewGuid())
                .Options;
            _context = new DbHmContext(options);
            _repository = new PatientRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CreatePatient_Should_AddPatient_ToDatabase()
        {
            // Arrange
            var patient = new Paciente
            {
                IdUsuario = 300,
            };

            // Act
            var createdPatient = _repository.CreatePatient(patient);

            // Assert
            Assert.NotEqual(0, createdPatient.Id);
            var patientInDb = _context.Paciente.Find(createdPatient.Id);
            Assert.NotNull(patientInDb);
            Assert.Equal(300, patientInDb.IdUsuario);
            Assert.True(patientInDb.DataCriacao != default(DateTime));
            Assert.True(patientInDb.DataModificacao != default(DateTime));
        }

        [Fact]
        public void CreatePatient_Should_ThrowArgumentException_WhenPatientIsNull()
        {
            // Arrange
            Paciente patient = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _repository.CreatePatient(patient));
        }

        [Fact]
        public void GetPatientByUserId_Should_ReturnPatient_WhenExists()
        {
            // Arrange
            var patient = new Paciente
            {
                IdUsuario = 400,
                DataCriacao = DateTime.Now,
                DataModificacao = DateTime.Now
            };
            _context.Paciente.Add(patient);
            _context.SaveChanges();

            // Act
            var retrievedPatient = _repository.GetPatientByUserId(400);

            // Assert
            Assert.NotNull(retrievedPatient);
            Assert.Equal(400, retrievedPatient.IdUsuario);
        }

        [Fact]
        public void GetPatientByUserId_Should_ReturnNull_WhenPatientDoesNotExist()
        {
            // Act
            var retrievedPatient = _repository.GetPatientByUserId(9999);

            // Assert
            Assert.Null(retrievedPatient);
        }
    }
}
