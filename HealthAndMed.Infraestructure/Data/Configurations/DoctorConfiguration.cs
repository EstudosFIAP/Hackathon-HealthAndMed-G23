using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HealthAndMed.Core.Entities;

public class DoctorConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.ToTable("medicos");

        builder.HasKey(e => e.Id).HasName("PK__medico__E038EB43B2915E5E");

        builder.Property(e => e.Id)
            .HasColumnName("id_medico");

        builder.Property(e => e.IdUsuario)
            .HasColumnName("id_usuario");

        builder.Property(e => e.NumeroCrm)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("crm_medico");

        builder.Property(e => e.Especialidade)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("especialidade");

        builder.Property(e => e.ValorConsulta)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("valor_consulta");

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
            .HasConstraintName("FK__medico__id_usuar__6CA31EA0");
    }
}

