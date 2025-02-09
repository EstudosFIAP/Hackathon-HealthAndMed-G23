using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthAndMed.API.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DbHmContext _context;

        public UserRepository(DbHmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Usuario CreateUser(Usuario userData)
        {
            if (userData == null)
                throw new ArgumentException("Você precisa informar os dados do usuário para cadastro.");

            userData.Data_Criacao = DateTime.Now;
            userData.Data_Modificacao = DateTime.Now;
            _context.Usuario.Add(userData);
            _context.SaveChanges();

            return userData;
        }

        public void DeleteUser(int Id)
        {
            var user = _context.Usuario.FirstOrDefault(u => u.Id == Id);
            if (user == null)
                throw new NotImplementedException();

            _context.Usuario.Remove(user);
            _context.SaveChanges();
        }

        public IEnumerable<UserDTO> ListUsers()
        {
            var query = from usuario in _context.Usuario                        
                        select new UserDTO
                        {
                            Id = usuario.Id,
                            Nome = usuario.Nome!,
                            Cpf = usuario.Cpf!,
                            Email = usuario.Email!,
                            Senha = usuario.Senha!,
                            TipoUsuario = usuario.TipoUsuario!,
                            DataCriacao = usuario.Data_Criacao,
                            DataModificacao = usuario.Data_Modificacao
                        };

            return query.Distinct().ToList();
        }

        public void UpdateUser(Usuario userData)
        {
            if (userData == null)
                throw new ArgumentException("Você precisa informar os dados do usuário para atualização");

            var existingUser = _context.Usuario.FirstOrDefault(u =>u.Id == userData.Id);
            if (existingUser == null)
                throw new ArgumentException("Usuário não encontrado");

            existingUser.Data_Modificacao = DateTime.Now;
            existingUser.Nome = userData.Nome != null ? userData.Nome : existingUser.Nome;
            existingUser.Cpf = userData.Cpf != null ? userData.Cpf : existingUser.Cpf;
            existingUser.Email = userData.Email != null ? userData.Email : existingUser.Email;
            existingUser.Senha = userData.Senha != null ? userData.Senha : existingUser.Senha;
            existingUser.TipoUsuario = userData.TipoUsuario !=null ? userData.TipoUsuario : existingUser.TipoUsuario;
            
            _context.Entry(existingUser).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}