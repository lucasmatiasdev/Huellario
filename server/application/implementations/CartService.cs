using application.interfaces;
using application.dtos.CartItem;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(int userId, string sessionId)
    {
        var items = await _unitOfWork.CartItems.GetItemsAsync(userId, sessionId);
        var itemsDto = items.Adapt<IEnumerable<CartItemDto>>();
        return new CartDto { Items = itemsDto.ToList() };
    }

    public async Task<CartDto> AddItemAsync(int userId, CreateCartItemDto dto)
    {
        var variant = await _unitOfWork.Products.GetVariantByIdAsync(dto.VariantId);
        if (variant == null)
            throw new InvalidOperationException("Producto sin stock disponible");

        var existing = (await _unitOfWork.CartItems.GetItemsAsync(userId, dto.SessionId ?? string.Empty))
            .FirstOrDefault(i => i.ProductId == dto.ProductId && i.VariantId == dto.VariantId);

        var totalQty = existing != null ? existing.Quantity + dto.Quantity : dto.Quantity;
        if (variant.Stock < totalQty)
            throw new InvalidOperationException("Producto sin stock disponible");

        var item = dto.Adapt<CartItem>();
        item.UserId = userId;
        item.SessionId = dto.SessionId ?? string.Empty;

        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
            await _unitOfWork.CartItems.UpdateQuantityAsync(existing);
        }
        else
        {
            await _unitOfWork.CartItems.AddItemAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetCartAsync(userId, dto.SessionId ?? string.Empty);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, string sessionId, int productId, int variantId)
    {
        var items = await _unitOfWork.CartItems.GetItemsAsync(userId, sessionId);
        var item = items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);
        if (item != null)
        {
            await _unitOfWork.CartItems.RemoveItemAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCartAsync(userId, sessionId);
    }

    public async Task<CartDto> UpdateQuantityAsync(int userId, UpdateCartItemDto dto)
    {
        var variant = await _unitOfWork.Products.GetVariantByIdAsync(dto.VariantId);
        if (variant == null || variant.Stock < dto.Quantity)
            throw new InvalidOperationException("Producto sin stock disponible");

        var items = await _unitOfWork.CartItems.GetItemsAsync(userId, dto.SessionId ?? string.Empty);
        var item = items.FirstOrDefault(i => i.ProductId == dto.ProductId && i.VariantId == dto.VariantId);
        if (item != null)
        {
            item.Quantity = dto.Quantity;
            await _unitOfWork.CartItems.UpdateQuantityAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCartAsync(userId, dto.SessionId ?? string.Empty);
    }

    public async Task<CartDto> ClearCartAsync(int userId, string sessionId)
    {
        await _unitOfWork.CartItems.ClearCartAsync(userId, sessionId);
        await _unitOfWork.SaveChangesAsync();
        return new CartDto();
    }

    public async Task<CartDto> TransferCartAsync(int userId, string sessionId)
    {
        var anonymousItems = await _unitOfWork.CartItems.GetItemsAsync(0, sessionId);
        if (!anonymousItems.Any())
            return await GetCartAsync(userId, string.Empty);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userItems = await _unitOfWork.CartItems.GetItemsAsync(userId, string.Empty);

            foreach (var anonItem in anonymousItems)
            {
                var variant = await _unitOfWork.Products.GetVariantByIdAsync(anonItem.VariantId);
                if (variant == null || variant.Stock < anonItem.Quantity)
                    throw new InvalidOperationException("Producto sin stock disponible");

                var existing = userItems.FirstOrDefault(i =>
                    i.ProductId == anonItem.ProductId && i.VariantId == anonItem.VariantId);

                if (existing != null)
                {
                    existing.Quantity += anonItem.Quantity;
                    await _unitOfWork.CartItems.UpdateQuantityAsync(existing);
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
                    await _unitOfWork.CartItems.AddItemAsync(newItem);
                }
            }

            await _unitOfWork.CartItems.ClearCartAsync(0, sessionId);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return await GetCartAsync(userId, string.Empty);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
