using application.interfaces;
using domain.dtos.Address;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(IAddressRepository addressRepository, IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);
        return addresses.Adapt<IEnumerable<AddressDto>>();
    }

    public async Task<AddressDto> GetByIdAsync(int userId, int id)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");
        return address.Adapt<AddressDto>();
    }

    public async Task<AddressDto> AddAsync(int userId, CreateAddressDto dto)
    {
        var count = await _addressRepository.GetCountByUserIdAsync(userId);
        if (count >= 5)
            throw new InvalidOperationException("No se pueden tener más de 5 direcciones guardadas");

        var address = dto.Adapt<Address>();
        address.UserId = userId;

        if (address.IsDefault)
        {
            var currentDefault = await _addressRepository.GetDefaultByUserIdAsync(userId);
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                await _addressRepository.UpdateAsync(currentDefault);
            }
        }

        await _addressRepository.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();
        return address.Adapt<AddressDto>();
    }

    public async Task UpdateAsync(int userId, int id, UpdateAddressDto dto)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");

        var wasDefault = address.IsDefault;
        dto.Adapt(address);

        if (address.IsDefault && !wasDefault)
        {
            var currentDefault = await _addressRepository.GetDefaultByUserIdAsync(userId);
            if (currentDefault != null && currentDefault.Id != id)
            {
                currentDefault.IsDefault = false;
                await _addressRepository.UpdateAsync(currentDefault);
            }
        }

        await _addressRepository.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");

        await _addressRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AddressDto> SetDefaultAsync(int userId, int id)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");

        var currentDefault = await _addressRepository.GetDefaultByUserIdAsync(userId);
        if (currentDefault != null && currentDefault.Id != id)
        {
            currentDefault.IsDefault = false;
            await _addressRepository.UpdateAsync(currentDefault);
        }

        address.IsDefault = true;
        await _addressRepository.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();
        return address.Adapt<AddressDto>();
    }
}
