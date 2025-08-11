using Order.Data;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            return orders;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }

        public async Task<(OrderDetail Order, string Error)> CreateOrderAsync(CreateOrderRequest request)
        {
            var order = await _orderRepository.CreateOrderAsync(request);
            return order;
        }

        public async Task<IEnumerable<Model.OrderItem>> GetProductsAsync()
        {
            var products = await _orderRepository.GetProductsAsync();
            return products;
        }

        public async Task<IEnumerable<MonthlyProfitRequest>> GetMonthlyProfitAsync()
        {
            var monthlyProfit = await _orderRepository.GetMonthlyProfitAsync();
            return monthlyProfit;
        }
    }
}
