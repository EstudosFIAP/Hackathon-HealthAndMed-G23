using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HealthAndMed.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Infraestructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("agendamentos");

        builder.HasKey(e => e.Id).HasName("PK__agendame__4EBA5535254C9623");

        builder.Property(e => e.Id).HasColumnName("id_agendamento");

        builder.Property(e => e.IdPaciente).HasColumnName("id_paciente");

        builder.Property(e => e.IdMedico).HasColumnName("id_medico");

        builder.Property(e => e.IdAgenda).HasColumnName("id_agenda");

        builder.Property(e => e.StatusAgendamento)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status_agendamento");

        builder.Property(e => e.MotivoCancelamento)
                .HasMaxLength(4000)
                .IsUnicode(false)
                .HasColumnName("motivo_cancelamento");

        builder.HasOne(e => e.Agenda)
                .WithMany()
                .HasForeignKey(e => e.IdAgenda)
                .HasConstraintName("FK__agendamen__id_ag__7908F585");

        builder.HasOne(d => d.Paciente).WithMany(p => p.Agendamentos)
            .HasForeignKey(d => d.IdPaciente)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK__agendamen__id_pa__79FD19BE");

        builder.HasOne(d => d.Medico).WithMany(p => p.Agendamentos)
            .HasForeignKey(d => d.IdMedico)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK__agendamen__id_me__7814D14C");
    }
}
