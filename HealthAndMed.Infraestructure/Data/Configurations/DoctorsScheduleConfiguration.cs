using HealthAndMed.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Infraestructure.Data.Configurations
{
    public class DoctorsScheduleConfiguration : IEntityTypeConfiguration<Agenda>
    {
        public void Configure(EntityTypeBuilder<Agenda> builder)
        {
            builder.ToTable("agendas");

            builder.HasKey(e => e.Id).HasName("PK__agenda__178E6FBB91FDB386");

            builder.Property(e => e.Id).HasColumnName("id_agenda");

            builder.Property(e => e.IdMedico).HasColumnName("id_medico");

            builder.Property(e => e.HorarioInicio)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("horario_inicio");

            builder.Property(e => e.HorarioFim)
                    .HasMaxLength(4000)
                    .IsUnicode(false)
                    .HasColumnName("horario_fim");

            builder.Property(e => e.IsBloqueado)
                    .HasMaxLength(4000)
                    .IsUnicode(false)
                    .HasColumnName("is_bloqueado");

            builder.HasOne(e => e.Medico).WithMany(p => p.Agendas)
                    .HasForeignKey(d => d.IdMedico)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__agenda__id_medic__725BF7F6");
        }
    }
}