using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebService.Model;

namespace WebService.Services;

public class OrdersRepository : IOrdersRepository
{
    private readonly NorthwindContext context;
    private readonly IOrderDetailsRepository repository;

    public OrdersRepository(NorthwindContext context, IOrderDetailsRepository repository)
    {
        this.context = context;
        this.repository = repository;
    }

    public IEnumerable<Order> GetAllOrders() => context.Orders.AsNoTracking().ToArray();

    public Order? GetOrderByID(int? id)
    {
        return context.Orders
            .Include(o => o.OrderDetails)
            .Include(nameof(Order.OrderDetails) + "." + nameof(OrderDetail.Product))
            .FirstOrDefault(o => o.OrderId == id);
    }

    public Order CreateNewOrder(Order newOrder)
    {
        context.Add(newOrder);
        context.SaveChanges();
        return newOrder;
    }

    public async Task<Order> UpdateOrder(int id, Order updatedOrder)
    {
        var existedOrder = GetOrderByID(id);
        if(existedOrder == null)
        {
            throw new ArgumentException("No order exists with the given id", nameof(id));
        }
        else
        {
            existedOrder.OrderDate = updatedOrder.OrderDate;
            existedOrder.RequiredDate = updatedOrder.RequiredDate;
            existedOrder.ShippedDate = updatedOrder.ShippedDate;
            existedOrder.Freight = updatedOrder.Freight;
            existedOrder.ShipName = updatedOrder.ShipName;
            existedOrder.ShipAddress = updatedOrder.ShipAddress;
            existedOrder.ShipCity = updatedOrder.ShipCity;
            existedOrder.ShipRegion = updatedOrder.ShipRegion;
            existedOrder.ShipPostalCode = updatedOrder.ShipPostalCode;
            existedOrder.ShipCountry = updatedOrder.ShipCountry;
            await context.SaveChangesAsync();
            return existedOrder;
        }
    }

    public async Task RemoveOrder(int? id)
    {
        var orderToDelete = GetOrderByID(id);
        if (orderToDelete == null)
        {
            throw new ArgumentException("No order exists with the given id", nameof(id));
        }

        //using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if(orderToDelete.OrderDetails != null)
            {
                foreach (OrderDetail item in orderToDelete.OrderDetails)
                {
                    if(item.OrderId != null && item.ProductId != null)
                    {
                        await repository.RemoveOrderDetail((int)item.OrderId, (int)item.ProductId);
                    }
                }
            }
            context.Remove(orderToDelete);
            await context.SaveChangesAsync();

            //await transaction.CommitAsync();
        }
        catch (SqlException ex)
        {
            throw ex;
        }
        catch(ArgumentException ex)
        {
            throw new ArgumentException(ex.ToString());
        }
        catch(Exception)
        {
            throw;
        }
    }
}
