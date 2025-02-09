using HealthAndMed.Core.DTO;
using HealthAndMed.Core.Entities;

namespace HealthAndMed.API.Repository.Interfaces;

public interface IUserRepository
{
    IEnumerable<UserDTO> ListUsers();
    Usuario CreateUser(Usuario userData);
    void UpdateUser(Usuario userData);
    void DeleteUser(int Id);
}