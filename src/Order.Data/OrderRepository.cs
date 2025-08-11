using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Where(x => x.Status.Name == status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .SingleOrDefaultAsync();

            var s = await _orderContext.OrderStatus
            .SingleOrDefaultAsync(s => s.Name == status);

            order.StatusId = s.Id;

            await _orderContext.SaveChangesAsync();
            return;
        }

        public async Task<(OrderDetail Order, string Error)> CreateOrderAsync(CreateOrderRequest request)
        {
            if (request is null) return (null, "Body required.");
            if (request.Items is null || request.Items.Count == 0)
                return (null, "At least one item is required.");
            if (request.Items.Any(i => i.Quantity <= 0))
                return (null, "Quantity must be positive.");

            var ctx = _orderContext;

            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = new List<OrderProduct>();

            foreach (var pid in productIds)
            {
                var idBytes = pid.ToByteArray();
                var prod = await ctx.OrderProduct
                    .Include(p => p.Service)
                    .SingleOrDefaultAsync(p => p.Id == idBytes);

                if (prod == null)
                    return (null, "One or more products not found.");

                products.Add(prod);
            }

            var createdStatus = await ctx.OrderStatus.FirstOrDefaultAsync(s => s.Name == "Created");
            if (createdStatus == null)
                return (null, "Status 'Created' is not configured.");

            var newOrderId = Guid.NewGuid();
            var orderEntity = new Entities.Order
            {
                Id = newOrderId.ToByteArray(),
                CustomerId = request.CustomerId.ToByteArray(),
                ResellerId = request.ResellerId.ToByteArray(),
                StatusId = createdStatus.Id,
                CreatedDate = DateTime.UtcNow
            };

            ctx.Order.Add(orderEntity);

            var items = new List<Model.OrderItem>();

            foreach (var item in request.Items)
            {
                var prod = products.First(p => p.Id.SequenceEqual(item.ProductId.ToByteArray()));

                var itemId = Guid.NewGuid();

                ctx.OrderItem.Add(new Entities.OrderItem
                {
                    Id = itemId.ToByteArray(),
                    OrderId = orderEntity.Id,
                    ProductId = prod.Id,
                    ServiceId = prod.ServiceId,
                    Quantity = item.Quantity
                });

                items.Add(new Model.OrderItem
                {
                    Id = itemId,
                    OrderId = newOrderId,
                    ServiceId = new Guid(prod.ServiceId),
                    ServiceName = prod.Service?.Name,
                    ProductId = new Guid (prod.Id),
                    ProductName = prod.Name,
                    UnitCost = prod.UnitCost,
                    UnitPrice = prod.UnitPrice,
                    Quantity = item.Quantity,
                    TotalCost = prod.UnitCost * item.Quantity,
                    TotalPrice = prod.UnitPrice * item.Quantity
                });
            }

            await ctx.SaveChangesAsync();

            var order = new OrderDetail
            {
                Id = newOrderId,
                ResellerId = request.ResellerId,
                CustomerId = request.CustomerId,
                StatusId = new Guid (createdStatus.Id),
                StatusName = createdStatus.Name,
                CreatedDate = orderEntity.CreatedDate,
                TotalCost = items.Sum(i => i.TotalCost),
                TotalPrice = items.Sum(i => i.TotalPrice),
                Items = items
            };

            return (order, null);
        }

        public async Task<IEnumerable<Model.OrderItem>> GetProductsAsync()
        {
            var products = await _orderContext.OrderItem
                .Include(x => x.Service)
                .Include(x => x.Product)
                .Select(x => new Model.OrderItem
                {
                    Id = new Guid(x.Id),
                    OrderId = new Guid(x.OrderId),
                    ServiceId = new Guid(x.ServiceId),
                    ServiceName = x.Service.Name,
                    ProductId = new Guid(x.ProductId),
                    ProductName = x.Product.Name,
                    Quantity = (int) x.Quantity,
                    UnitCost = x.Product.UnitCost,
                    UnitPrice = x.Product.UnitPrice,
                    TotalCost = (decimal) (x.Product.UnitCost * x.Quantity),
                    TotalPrice = (decimal) (x.Product.UnitPrice * x.Quantity)
                })
                .ToListAsync();

            return products;
        }

        public async Task<IEnumerable<MonthlyProfitRequest>> GetMonthlyProfitAsync()
        {
            var rawData = await _orderContext.Order
                .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                .Include(x => x.Status)
                .Where(x => x.Status.Name == "Completed")
                .Select(x => new
                {
                    x.CreatedDate,
                    Items = x.Items.Select(i => new
                    {
                        i.Quantity,
                        i.Product.UnitPrice,
                        i.Product.UnitCost
                    })
                })
                .ToListAsync();

            var monthlyProfit = rawData
                .SelectMany(order => order.Items.Select(i => new
                {
                    order.CreatedDate,
                    Profit = i.Quantity * (i.UnitPrice - i.UnitCost)
                }))
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .Select(g => new MonthlyProfitRequest
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Profit = (decimal)g.Sum(x => x.Profit)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            return monthlyProfit;
        }
    }
}
