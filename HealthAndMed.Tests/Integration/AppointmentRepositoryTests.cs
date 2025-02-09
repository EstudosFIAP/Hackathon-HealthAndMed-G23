using HealthAndMed.API.Repository;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Tests.Integration
{
    public class AppointmentRepositoryTests : IDisposable
    {
        private readonly DbHmContext _context;
        private readonly AppointmentRepository _repository;

        public AppointmentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DbHmContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            _context = new DbHmContext(options);
            _repository = new AppointmentRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CreateAppointment_Should_AddAppointment_ToDatabase()
        {
            // Arrange
            var appointment = new Agendamento
            {
                IdPaciente = 10,
                IdMedico = 20,
                IdAgenda = 30
            };

            // Act
            var createdAppointment = _repository.CreateAppointment(appointment);

            // Assert
            Assert.NotEqual(0, createdAppointment.Id);
            var appointmentInDb = _context.Agendamento.Find(createdAppointment.Id);
            Assert.NotNull(appointmentInDb);
            Assert.Equal(10, appointmentInDb.IdPaciente);
            Assert.Equal(20, appointmentInDb.IdMedico);
            Assert.Equal(30, appointmentInDb.IdAgenda);
            Assert.Equal("Pendente", appointmentInDb.StatusAgendamento);
        }

        [Fact]
        public void ListAppointments_Should_ReturnAddedAppointments()
        {
            // Arrange

            var usuarioPaciente = new Usuario { Nome = "Paciente 1" };
            var paciente = new Paciente { IdUsuario = 10, Usuario = usuarioPaciente };

            var usuarioMedico = new Usuario { Nome = "Medico 1" };
            var medico = new Medico { IdUsuario = 20, Usuario = usuarioMedico };

            // Também precisamos de uma agenda para o agendamento (para acessar HorarioInicio, por exemplo)
            var agendaEntity = new Agenda { HorarioInicio = DateTime.Now.AddHours(9) };

            var appointment = new Agendamento
            {
                IdPaciente = 10,
                IdMedico = 20,
                IdAgenda = 30,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now,
                StatusAgendamento = "Pendente",
                MotivoCancelamento = "",
                Paciente = paciente,
                Medico = medico,
                Agenda = agendaEntity
            };

            _context.Agendamento.Add(appointment);
            _context.SaveChanges();

            // Act
            var appointments = _repository.ListAppointments();

            // Assert
            Assert.NotEmpty(appointments);
            Assert.Single(appointments);
        }

        [Fact]
        public void UpdateAppointment_Should_ModifyExistingAppointment()
        {
            // Arrange
            var appointment = new Agendamento
            {
                IdPaciente = 10,
                IdMedico = 20,
                IdAgenda = 30,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now,
                StatusAgendamento = "Agendado",
                MotivoCancelamento = ""
            };
            _context.Agendamento.Add(appointment);
            _context.SaveChanges();

            var updatedAppointment = new Agendamento
            {
                Id = appointment.Id,
                IdPaciente = 11,
                IdMedico = 21,
                IdAgenda = 31,
            };

            // Act
            _repository.UpdateAppointment(updatedAppointment);

            // Assert
            var appointmentInDb = _context.Agendamento.Find(appointment.Id);
            Assert.Equal(11, appointmentInDb.IdPaciente);
            Assert.Equal(21, appointmentInDb.IdMedico);
            Assert.Equal(31, appointmentInDb.IdAgenda);
        }

        [Fact]
        public void DeleteAppointment_Should_RemoveAppointment_FromDatabase()
        {
            // Arrange
            var appointment = new Agendamento
            {
                IdPaciente = 10,
                IdMedico = 20,
                IdAgenda = 30,
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now,
                StatusAgendamento = "Agendado",
                MotivoCancelamento = ""
            };
            _context.Agendamento.Add(appointment);
            _context.SaveChanges();

            // Act
            _repository.DeleteAppointment(appointment.Id);

            // Assert
            var appointmentInDb = _context.Agendamento.Find(appointment.Id);
            Assert.Null(appointmentInDb);
        }

        [Fact]
        public void DeleteAppointment_Should_ThrowException_WhenAppointmentNotFound()
        {
            // Arrange
            int invalidId = 999;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => _repository.DeleteAppointment(invalidId));
        }
    }
}
