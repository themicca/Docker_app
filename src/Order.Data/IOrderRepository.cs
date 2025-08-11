using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<IEnumerable<OrderItem>> GetProductsAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status);

        Task UpdateOrderStatusAsync(Guid orderId, string status);

        Task<(OrderDetail Order, string Error)> CreateOrderAsync(CreateOrderRequest request);

        Task<IEnumerable<MonthlyProfitRequest>> GetMonthlyProfitAsync();
    }
}
