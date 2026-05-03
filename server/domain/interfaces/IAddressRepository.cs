using domain.entities;

namespace domain.interfaces;

public interface IAddressRepository
{
    Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
    Task<Address?> GetByIdAsync(int id);
    Task<int> GetCountByUserIdAsync(int userId);
    Task<Address?> GetDefaultByUserIdAsync(int userId);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(int id);
}
