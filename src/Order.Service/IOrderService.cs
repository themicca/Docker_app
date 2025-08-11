using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<IEnumerable<Model.OrderItem>> GetProductsAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status);

        Task UpdateOrderStatusAsync(Guid orderId, string status);

        Task<(OrderDetail Order, string Error)> CreateOrderAsync(CreateOrderRequest request);

        Task<IEnumerable<MonthlyProfitRequest>> GetMonthlyProfitAsync();
    }
}
