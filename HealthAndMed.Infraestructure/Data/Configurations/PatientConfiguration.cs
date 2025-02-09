using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HealthAndMed.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Infraestructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("pacientes");

        builder.HasKey(e => e.Id).HasName("PKpaciente2C2C72BB2AB9C7D9");

        builder.Property(e => e.Id).HasColumnName("id_paciente");

        builder.Property(e => e.IdUsuario)
        .HasColumnName("id_usuario");

        builder.Property(e => e.DataCriacao)
        .HasColumnType("datetime")
        .HasColumnName("data_criacao")
        .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.DataModificacao)
        .HasColumnType("datetime")
        .HasColumnName("data_modificacao")
        .HasDefaultValueSql("GETDATE()");

        builder.HasOne(e => e.Usuario)
        .WithMany()
        .HasForeignKey(d => d.IdUsuario)
        .OnDelete(DeleteBehavior.Cascade)
        .HasConstraintName("FK__paciente__id_usu__66EA454A");
    }
}