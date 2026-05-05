using application.interfaces;
using domain.dtos.Address;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class AddressService : IAddressService
{
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId)
    {
        var addresses = await _unitOfWork.Addresses.GetByUserIdAsync(userId);
        return addresses.Adapt<IEnumerable<AddressDto>>();
    }

    public async Task<AddressDto> GetByIdAsync(int userId, int id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");
        return address.Adapt<AddressDto>();
    }

    public async Task<AddressDto> AddAsync(int userId, CreateAddressDto dto)
    {
        var count = await _unitOfWork.Addresses.GetCountByUserIdAsync(userId);
        if (count >= 5)
            throw new InvalidOperationException("No se pueden tener más de 5 direcciones guardadas");

        var address = dto.Adapt<Address>();
        address.UserId = userId;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (address.IsDefault)
            {
                var currentDefault = await _unitOfWork.Addresses.GetDefaultByUserIdAsync(userId);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                    await _unitOfWork.Addresses.UpdateAsync(currentDefault);
                }
            }

            await _unitOfWork.Addresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return address.Adapt<AddressDto>();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task UpdateAsync(int userId, int id, UpdateAddressDto dto)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");

        var wasDefault = address.IsDefault;
        dto.Adapt(address);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (address.IsDefault && !wasDefault)
            {
                var currentDefault = await _unitOfWork.Addresses.GetDefaultByUserIdAsync(userId);
                if (currentDefault != null && currentDefault.Id != id)
                {
                    currentDefault.IsDefault = false;
                    await _unitOfWork.Addresses.UpdateAsync(currentDefault);
                }
            }

            await _unitOfWork.Addresses.UpdateAsync(address);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");

        await _unitOfWork.Addresses.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AddressDto> SetDefaultAsync(int userId, int id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            throw new KeyNotFoundException("Dirección no encontrada");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var currentDefault = await _unitOfWork.Addresses.GetDefaultByUserIdAsync(userId);
            if (currentDefault != null && currentDefault.Id != id)
            {
                currentDefault.IsDefault = false;
                await _unitOfWork.Addresses.UpdateAsync(currentDefault);
            }

            address.IsDefault = true;
            await _unitOfWork.Addresses.UpdateAsync(address);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return address.Adapt<AddressDto>();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
