using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebService.Model;

namespace WebService.Services;

public interface IOrderDetailsRepository
{
    OrderDetail? GetOrderDetailByID(int? orderID, int? productID);
    Task<OrderDetail?> AddProductToOrder(OrderDetail newOrderDetail);
    Task RemoveOrderDetail(int orderId, int productId);
    Task<OrderDetail?> UpdateOrderDetail(int orderId, int productId, JsonPatchDocument<OrderDetail> patchDoc, ModelStateDictionary ModelState);
}
