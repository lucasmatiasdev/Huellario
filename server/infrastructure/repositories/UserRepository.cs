using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class UserRepository : IUserRepository
{
    private readonly HuellarioDbContext _context;

    public UserRepository(HuellarioDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByIdentityIdAsync(string identityId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }
}
