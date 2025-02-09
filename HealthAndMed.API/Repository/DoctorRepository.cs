using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Infraestructure.Data;
using HealthAndMed.Core.Entities;
using Microsoft.EntityFrameworkCore;
public class DoctorRepository : IDoctorRepository
{
    private readonly DbHmContext _context;

    public DoctorRepository(DbHmContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Medico CreateDoctor(Medico doctorData)
    {
        if (doctorData == null)
            throw new ArgumentException("Você precisa informar os dados do médico para cadastro.");

        doctorData.DataCriacao = DateTime.Now;
        doctorData.DataModificacao = DateTime.Now;

        _context.Medico.Add(doctorData);
        _context.SaveChanges();

        return doctorData;
    }


    public Medico? GetDoctorByUserId(int userId)
    {
        return _context.Medico.FirstOrDefault(d => d.IdUsuario == userId);
    }
    public IEnumerable<Medico> GetAllDoctors()
    {
        return _context.Medico.Include(d => d.Usuario).ToList();
    }
}
