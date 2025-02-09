using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;

namespace HealthAndMed.API.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly DbHmContext _context;

        public PatientRepository(DbHmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Paciente CreatePatient(Paciente patientData)
        {
            if (patientData == null)
                throw new ArgumentException("Você precisa informar os dados do paciente para cadastro");

            patientData.DataCriacao = DateTime.Now;
            patientData.DataModificacao = DateTime.Now;

            _context.Paciente.Add(patientData);
            _context.SaveChanges();

            return patientData;
        }

        public Paciente? GetPatientByUserId(int userId)
        {
            return _context.Paciente.FirstOrDefault(p => p.IdUsuario == userId);
        }
    }
}
