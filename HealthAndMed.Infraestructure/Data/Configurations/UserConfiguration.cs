using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HealthAndMed.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.Infraestructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(e => e.Id).HasName("PK__usuario__4E3E04ADBF262E99");

        builder.Property(e => e.Id).HasColumnName("id_usuario");

        builder.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nome_completo");

        builder.Property(e => e.Cpf)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("cpf_usuario");

        builder.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email_usuario");

        builder.Property(e => e.Senha)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("senha_hash");

        builder.Property(e => e.TipoUsuario)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("tipo_usuario");
    }
}
