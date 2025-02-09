using HealthAndMed.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HealthAndMed.Infraestructure.Data
{
    public class DbHmContext : DbContext
    {
        public DbHmContext(DbContextOptions<DbHmContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Paciente> Paciente { get; set; }
        public virtual DbSet<Medico> Medico { get; set; }
        public virtual DbSet<Agenda> Agenda { get; set; }
        public virtual DbSet<Agendamento> Agendamento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}