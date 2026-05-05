using application.interfaces;
using domain.dtos.CartItem;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(ICartRepository cartRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(int userId, string sessionId)
    {
        var items = await _cartRepository.GetItemsAsync(userId, sessionId);
        var itemsDto = items.Adapt<IEnumerable<CartItemDto>>();
        return new CartDto { Items = itemsDto.ToList() };
    }

    public async Task<CartDto> AddItemAsync(int userId, CreateCartItemDto dto)
    {
        var item = dto.Adapt<CartItem>();
        item.UserId = userId;
        item.SessionId = dto.SessionId ?? string.Empty;

        var existing = (await _cartRepository.GetItemsAsync(userId, dto.SessionId ?? string.Empty))
            .FirstOrDefault(i => i.ProductId == dto.ProductId && i.VariantId == dto.VariantId);

        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
            await _cartRepository.UpdateQuantityAsync(existing);
        }
        else
        {
            await _cartRepository.AddItemAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetCartAsync(userId, dto.SessionId ?? string.Empty);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, string sessionId, int productId, int variantId)
    {
        var items = await _cartRepository.GetItemsAsync(userId, sessionId);
        var item = items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);
        if (item != null)
        {
            await _cartRepository.RemoveItemAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCartAsync(userId, sessionId);
    }

    public async Task<CartDto> UpdateQuantityAsync(int userId, UpdateCartItemDto dto)
    {
        var items = await _cartRepository.GetItemsAsync(userId, dto.SessionId ?? string.Empty);
        var item = items.FirstOrDefault(i => i.ProductId == dto.ProductId && i.VariantId == dto.VariantId);
        if (item != null)
        {
            item.Quantity = dto.Quantity;
            await _cartRepository.UpdateQuantityAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCartAsync(userId, dto.SessionId ?? string.Empty);
    }

    public async Task<CartDto> ClearCartAsync(int userId, string sessionId)
    {
        await _cartRepository.ClearCartAsync(userId, sessionId);
        await _unitOfWork.SaveChangesAsync();
        return new CartDto();
    }

    public async Task<CartDto> TransferCartAsync(int userId, string sessionId)
    {
        var anonymousItems = await _cartRepository.GetItemsAsync(0, sessionId);
        if (!anonymousItems.Any())
            return await GetCartAsync(userId, string.Empty);

        var userItems = await _cartRepository.GetItemsAsync(userId, string.Empty);

        foreach (var anonItem in anonymousItems)
        {
            var existing = userItems.FirstOrDefault(i =>
                i.ProductId == anonItem.ProductId && i.VariantId == anonItem.VariantId);

            if (existing != null)
            {
                existing.Quantity += anonItem.Quantity;
                await _cartRepository.UpdateQuantityAsync(existing);
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    SessionId = string.Empty,
                    ProductId = anonItem.ProductId,
                    VariantId = anonItem.VariantId,
                    Quantity = anonItem.Quantity
                };
                await _cartRepository.AddItemAsync(newItem);
            }
        }

        await _cartRepository.ClearCartAsync(0, sessionId);
        await _unitOfWork.SaveChangesAsync();
        return await GetCartAsync(userId, string.Empty);
    }
}
