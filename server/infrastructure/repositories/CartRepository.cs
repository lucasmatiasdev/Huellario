using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class CartRepository : ICartRepository
{
    private readonly HuellarioDbContext _context;

    public CartRepository(HuellarioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItem>> GetItemsAsync(int userId, string sessionId)
    {
        if (userId > 0)
            return await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .Where(c => c.UserId == userId)
                .ToListAsync();

        return await _context.CartItems
            .Include(c => c.Product)
            .Include(c => c.Variant)
            .Where(c => c.SessionId == sessionId && c.UserId == 0)
            .ToListAsync();
    }

    public async Task AddItemAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        var existing = await _context.CartItems
            .FindAsync(item.UserId, item.SessionId, item.ProductId, item.VariantId);
        if (existing != null)
        {
            _context.CartItems.Remove(existing);
        }
    }

    public async Task UpdateQuantityAsync(CartItem item)
    {
        _context.CartItems.Update(item);
    }

    public async Task ClearCartAsync(int userId, string sessionId)
    {
        List<CartItem> items;

        if (userId > 0)
            items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();
        else
            items = await _context.CartItems
                .Where(c => c.SessionId == sessionId && c.UserId == 0)
                .ToListAsync();

        _context.CartItems.RemoveRange(items);
    }
}
