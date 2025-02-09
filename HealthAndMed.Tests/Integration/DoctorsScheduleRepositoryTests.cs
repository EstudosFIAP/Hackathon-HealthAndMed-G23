using HealthAndMed.API.Repository;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Tests.Integration
{
    public class DoctorsScheduleRepositoryTests : IDisposable
    {
        private readonly DbHmContext _context;
        private readonly DoctorsScheduleRepository _repository;

        public DoctorsScheduleRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DbHmContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            _context = new DbHmContext(options);
            _repository = new DoctorsScheduleRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CreateDoctorsSchedule_Should_AddSchedule_ToDatabase()
        {
            // Arrange
            var agenda = new Agenda
            {
                HorarioInicio = DateTime.Today.AddHours(9),
                HorarioFim = DateTime.Today.AddHours(11)
            };

            // Act
            var createdAgenda = _repository.CreateDoctorsSchedule(agenda);

            // Assert
            Assert.NotEqual(0, createdAgenda.Id);
            var agendaInDb = _context.Agenda.Find(createdAgenda.Id);
            Assert.NotNull(agendaInDb);
            Assert.Equal(DateTime.Today.AddHours(9), agendaInDb.HorarioInicio);
            Assert.Equal(DateTime.Today.AddHours(11), agendaInDb.HorarioFim);
        }

        [Fact]
        public void ListDoctorsSchedule_Should_ReturnAddedSchedules()
        {
            // Arrange: criar o objeto Usuario e o Medico associado
            var usuarioMedico = new Usuario
            {
                Nome = "Dr. Test",
            };

            var medico = new Medico
            {
                Id = 10,
                IdUsuario = 100,
                Usuario = usuarioMedico,
                Especialidade = "Cardiologia",
                ValorConsulta = 150.00m
            };

            // Use uma variável para a data atual para manter consistência
            var now = DateTime.Now;
            var agenda = new Agenda
            {
                IdMedico = medico.Id,
                HorarioInicio = now.AddHours(9),
                HorarioFim = now.AddHours(11),
                IsBloqueado = false,
                Data_Criacao = now,
                Data_Modificacao = now,
                Medico = medico // Preenche a propriedade de navegação
            };

            // Adiciona a agenda ao contexto e salva
            _context.Agenda.Add(agenda);
            _context.SaveChanges();

            // Act: chama o método que projeta para DoctorsScheduleDTO
            var schedules = _repository.ListDoctorsSchedule();

            // Assert: verifica se a coleção não está vazia e contém exatamente um item
            Assert.NotEmpty(schedules);
            Assert.Single(schedules);

            // Valida os dados do DTO projetado
            var scheduleDto = schedules.First();
            Assert.Equal(agenda.Id, scheduleDto.Id);
            Assert.Equal(agenda.IdMedico, scheduleDto.IdMedico);
            Assert.Equal("Dr. Test", scheduleDto.Medico);
            Assert.Equal("Cardiologia", scheduleDto.Especialidade);
            Assert.Equal(150.00m, scheduleDto.ValorConsulta);
            Assert.Equal(agenda.HorarioInicio, scheduleDto.HorarioInicio);
            Assert.Equal(agenda.HorarioFim, scheduleDto.HorarioFim);
            Assert.Equal(agenda.IsBloqueado, scheduleDto.IsBloqueado);
            Assert.Equal(agenda.Data_Criacao, scheduleDto.DataCriacao);
            Assert.Equal(agenda.Data_Modificacao, scheduleDto.DataModificacao);
        }

        [Fact]
        public void UpdateDoctorsSchedule_Should_ModifyExistingSchedule()
        {
            // Arrange
            var agenda = new Agenda
            {
                HorarioInicio = DateTime.Today.AddHours(9),
                HorarioFim = DateTime.Today.AddHours(11),
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now,
                IsBloqueado = false
            };
            _context.Agenda.Add(agenda);
            _context.SaveChanges();

            var updatedAgenda = new Agenda
            {
                Id = agenda.Id,
                HorarioInicio = DateTime.Today.AddHours(10),
                HorarioFim = DateTime.Today.AddHours(12)      
            };

            // Act
            _repository.UpdateDoctorsSchedule(updatedAgenda);

            // Assert
            var agendaInDb = _context.Agenda.Find(agenda.Id);
            Assert.Equal(DateTime.Today.AddHours(10), agendaInDb.HorarioInicio);
            Assert.Equal(DateTime.Today.AddHours(12), agendaInDb.HorarioFim);
        }

        [Fact]
        public void DeleteDoctorsSchedule_Should_RemoveSchedule_FromDatabase()
        {
            // Arrange
            var agenda = new Agenda
            {
                HorarioInicio = DateTime.Today.AddHours(9),
                HorarioFim = DateTime.Today.AddHours(11),
                Data_Criacao = DateTime.Now,
                Data_Modificacao = DateTime.Now,
                IsBloqueado = false
            };
            _context.Agenda.Add(agenda);
            _context.SaveChanges();

            // Act
            _repository.DeleteDoctorsSchedule(agenda.Id);

            // Assert
            var agendaInDb = _context.Agenda.Find(agenda.Id);
            Assert.Null(agendaInDb);
        }

        [Fact]
        public void DeleteDoctorsSchedule_Should_ThrowException_WhenScheduleNotFound()
        {
            // Arrange
            int invalidId = 999;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => _repository.DeleteDoctorsSchedule(invalidId));
        }
    }
}
