using domain.dtos.CartItem;

namespace application.interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId, string sessionId);
    Task<CartDto> AddItemAsync(int userId, CreateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(int userId, string sessionId, int productId, int variantId);
    Task<CartDto> UpdateQuantityAsync(int userId, UpdateCartItemDto dto);
    Task<CartDto> ClearCartAsync(int userId, string sessionId);
    Task<CartDto> TransferCartAsync(int userId, string sessionId);
}
