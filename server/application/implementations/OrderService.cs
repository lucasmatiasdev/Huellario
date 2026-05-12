using application.interfaces;
using application.dtos.Order;
using domain.entities;
using domain.enums;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> _validTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Shipping, OrderStatus.Cancelled],
        [OrderStatus.Shipping] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = [],
        [OrderStatus.Refunded] = []
    };

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, string sessionId, CreateOrderDto dto)
    {
        var cartItems = (await _unitOfWork.CartItems.GetItemsAsync(userId, sessionId)).ToList();

        if (cartItems.Count == 0)
            throw new InvalidOperationException("El carrito está vacío");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                UserId = userId > 0 ? userId : null,
                SessionId = userId > 0 ? null : sessionId,
                AddressId = dto.IsPickup ? null : dto.AddressId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                IsPickup = dto.IsPickup,
                Note = dto.Note
            };

            foreach (var cartItem in cartItems)
            {
                var product = cartItem.Product
                    ?? throw new InvalidOperationException($"Producto {cartItem.ProductId} no encontrado");
                var variant = cartItem.Variant
                    ?? throw new InvalidOperationException($"Variante {cartItem.VariantId} no encontrada");

                if (variant.Stock < cartItem.Quantity)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para {product.Name} - {variant.Name}");

                variant.Stock -= cartItem.Quantity;

                var unitPrice = product.Price + (variant.Price ?? 0);

                order.OrderLines.Add(new OrderLine
                {
                    ProductId = cartItem.ProductId,
                    VariantId = cartItem.VariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                });
            }

            order.Total = order.OrderLines.Sum(ol => ol.Quantity * ol.UnitPrice);

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CartItems.ClearCartAsync(userId, sessionId);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            var orderDto = order.Adapt<OrderDto>();
            EnrichOrderDto(order, orderDto);
            return orderDto;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto> GetByIdAsync(int id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Pedido no encontrado");

        var orderDto = order.Adapt<OrderDto>();
        EnrichOrderDto(order, orderDto);
        return orderDto;
    }

    public async Task<IEnumerable<OrderListDto>> GetByUserAsync(int userId)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
        return orders.Select(order =>
        {
            var dto = order.Adapt<OrderListDto>();
            dto.ItemCount = order.OrderLines.Sum(ol => ol.Quantity);
            return dto;
        });
    }

    public async Task<IEnumerable<OrderListDto>> GetBySessionIdAsync(string sessionId)
    {
        var orders = await _unitOfWork.Orders.GetBySessionIdAsync(sessionId);
        return orders.Select(order =>
        {
            var dto = order.Adapt<OrderListDto>();
            dto.ItemCount = order.OrderLines.Sum(ol => ol.Quantity);
            dto.UserName = order.User != null
                ? $"{order.User.Name} {order.User.Surname}"
                : null;
            return dto;
        });
    }

    public async Task<IEnumerable<OrderListDto>> GetAllAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        return orders.Select(order =>
        {
            var dto = order.Adapt<OrderListDto>();
            dto.ItemCount = order.OrderLines.Sum(ol => ol.Quantity);
            dto.UserName = order.User != null
                ? $"{order.User.Name} {order.User.Surname}"
                : null;
            return dto;
        });
    }

    public async Task CancelAsync(int id, int userId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Pedido no encontrado");

        if (order.UserId != userId)
            throw new UnauthorizedAccessException("No puedes cancelar un pedido que no te pertenece");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Solo se pueden cancelar pedidos pendientes");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            order.Status = OrderStatus.Cancelled;

            foreach (var line in order.OrderLines)
            {
                if (line.Variant != null)
                {
                    line.Variant.Stock += line.Quantity;
                }
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task CancelBySessionAsync(int id, string sessionId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Pedido no encontrado");

        if (order.SessionId != sessionId)
            throw new UnauthorizedAccessException("No puedes cancelar un pedido que no te pertenece");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Solo se pueden cancelar pedidos pendientes");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            order.Status = OrderStatus.Cancelled;

            foreach (var line in order.OrderLines)
            {
                if (line.Variant != null)
                {
                    line.Variant.Stock += line.Quantity;
                }
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Pedido no encontrado");

        if (!_validTransitions.TryGetValue(order.Status, out var validTargets)
            || !validTargets.Contains(status))
        {
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de {order.Status} a {status}");
        }

        order.Status = status;
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var orderDto = order.Adapt<OrderDto>();
        EnrichOrderDto(order, orderDto);
        return orderDto;
    }

    private static void EnrichOrderDto(Order order, OrderDto dto)
    {
        dto.UserName = order.User != null
            ? $"{order.User.Name} {order.User.Surname}"
            : null;

        dto.AddressLine = order.Address != null
            ? $"{order.Address.Street} {order.Address.Number}, {order.Address.City}"
            : null;
    }
}
