using domain.entities;

namespace domain.interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByIdentityIdAsync(string identityId);
    Task AddAsync(User user);
}
