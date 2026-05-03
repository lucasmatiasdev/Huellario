using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class AddressRepository : IAddressRepository
{
    private readonly HuellarioDbContext _context;

    public AddressRepository(HuellarioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();
    }

    public async Task<Address?> GetByIdAsync(int id)
    {
        return await _context.Addresses.FindAsync(id);
    }

    public async Task<int> GetCountByUserIdAsync(int userId)
    {
        return await _context.Addresses.CountAsync(a => a.UserId == userId);
    }

    public async Task<Address?> GetDefaultByUserIdAsync(int userId)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
    }

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
    }

    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
    }

    public async Task DeleteAsync(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address != null)
        {
            _context.Addresses.Remove(address);
        }
    }
}
