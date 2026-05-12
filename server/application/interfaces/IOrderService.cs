using application.dtos.Order;
using domain.enums;

namespace application.interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, string sessionId, CreateOrderDto dto);
    Task<OrderDto> GetByIdAsync(int id);
    Task<IEnumerable<OrderListDto>> GetByUserAsync(int userId);
    Task<IEnumerable<OrderListDto>> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<OrderListDto>> GetAllAsync();
    Task CancelAsync(int id, int userId);
    Task CancelBySessionAsync(int id, string sessionId);
    Task<OrderDto> UpdateStatusAsync(int id, OrderStatus status);
}
