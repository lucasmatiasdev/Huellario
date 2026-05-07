using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class OrderRepository : IOrderRepository
{
    private readonly HuellarioDbContext _context;

    public OrderRepository(HuellarioDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Variant)
            .Include(o => o.User)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Variant)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Variant)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }
}
