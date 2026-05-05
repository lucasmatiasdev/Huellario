using domain.entities;

namespace domain.interfaces;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetItemsAsync(int userId, string sessionId);
    Task AddItemAsync(CartItem item);
    Task RemoveItemAsync(CartItem item);
    Task UpdateQuantityAsync(CartItem item);
    Task ClearCartAsync(int userId, string sessionId);
}