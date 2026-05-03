using domain.dtos.Address;

namespace application.interfaces;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId);
    Task<AddressDto> GetByIdAsync(int userId, int id);
    Task<AddressDto> AddAsync(int userId, CreateAddressDto dto);
    Task UpdateAsync(int userId, int id, UpdateAddressDto dto);
    Task DeleteAsync(int userId, int id);
    Task<AddressDto> SetDefaultAsync(int userId, int id);
}
