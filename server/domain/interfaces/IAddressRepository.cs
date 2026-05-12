using domain.entities;
using domain.enums;

namespace domain.interfaces;

public interface IAddressRepository
{
    Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
    Task<Address?> GetByIdAsync(int id);
    Task<int> GetCountByUserIdAsync(int userId);
    Task<Address?> GetDefaultByUserIdAsync(int userId, AddressType type);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(int id);
}
