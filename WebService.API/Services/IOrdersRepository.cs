using WebService.Model;

namespace WebService.Services;

public interface IOrdersRepository
{
    IEnumerable<Order> GetAllOrders();
    Order? GetOrderByID(int? id);
    Order CreateNewOrder(Order newOrder);
    Task<Order> UpdateOrder(int id, Order updatedOrder);
    Task RemoveOrder(int? id);
}
