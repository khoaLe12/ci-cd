using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebService.Model;

namespace WebService.Services;

public class OrderDetailsRepository : IOrderDetailsRepository
{
    private readonly NorthwindContext context;
    //private readonly IOrdersRepository repository;
    private readonly IProductsRepository productRepository;

    public OrderDetailsRepository(IProductsRepository productRepository, NorthwindContext context)
    {
        //this.repository = repository;
        this.productRepository = productRepository;
        this.context = context;
    }

    public OrderDetail? GetOrderDetailByID(int? orderID, int? productID) =>
        context.OrderDetails.FirstOrDefault(o => o.OrderId == orderID && o.ProductId == productID);

    public async Task<OrderDetail?> AddProductToOrder(OrderDetail newOrderDetail)
    {
        var product = productRepository.GetProductById(newOrderDetail.ProductId);
        if (context.Orders.FirstOrDefault(o => o.OrderId == newOrderDetail.OrderId) == null || product == null)
        {
            return null;
        }
        else
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.AddAsync(newOrderDetail);
                await context.SaveChangesAsync();

                product.UnitsInStock =  (short?)(product.UnitsInStock - newOrderDetail.Quantity);
                product.UnitsOnOrder = (short?)(product.UnitsOnOrder + newOrderDetail.Quantity);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch(SqlException ex)
            {
                throw new Exception("Error when saving data: " + ex.ToString());
            }
            return newOrderDetail;
        }
    }

    public async Task RemoveOrderDetail(int orderId, int productId)
    {
        var orderDetailToDelete = GetOrderDetailByID(orderId, productId);
        //var existingProduct = productRepository.GetProductById(productId);
        var existingProduct = context.Products.FirstOrDefault(p => p.ProductId == productId);

        if (orderDetailToDelete == null)
        {
            throw new ArgumentException("No Order Detail exists with the given id");
        }
        if (existingProduct == null)
        {
            throw new ArgumentException("No Product exists with the given id");
        }

        //Begin transaction
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var quantity = orderDetailToDelete.Quantity;

            //Remove
            context.Remove(orderDetailToDelete);
            await context.SaveChangesAsync();

            //Make a change on product
            existingProduct.UnitsInStock = (short?)(existingProduct.UnitsInStock + quantity);
            existingProduct.UnitsOnOrder = (short?)(existingProduct.UnitsOnOrder - quantity);
            await context.SaveChangesAsync();

            //Commit
            await transaction.CommitAsync();
        }
        catch (SqlException ex)
        {
            throw ex;
        }
    }

    public async Task<OrderDetail?> UpdateOrderDetail(int orderId, int productId, JsonPatchDocument<OrderDetail> patchDoc, ModelStateDictionary ModelState)
    {
        var orderDetailToUpdate = GetOrderDetailByID(orderId, productId);
        //var product = productRepository.GetProductById(productId);
        var product = context.Products.FirstOrDefault(p => p.ProductId == productId);

        if (orderDetailToUpdate == null || product == null)
        {
            return null;
        }
        else
        {
            var currentQuantity = orderDetailToUpdate.Quantity;
            var updatedQuantity = new short();

            //Define Delegate method that pass the error information from JsonPatchError to ModelState
            Action <JsonPatchError> errorHandler = (error) =>
            {
                var operation = patchDoc.Operations.FirstOrDefault(op => op.path == error.AffectedObject.ToString());
                if (operation != null)
                {
                    var propertyName = operation.path.Split('/').Last();
                    ModelState.AddModelError(propertyName, error.ErrorMessage);
                }
                else
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            };

            //Make a changes on quantity property field of Orde Detail
            patchDoc.ApplyTo(orderDetailToUpdate, errorHandler);
            if (!ModelState.IsValid)
            {
                throw new Exception(ModelState.ToString());
            }

            //Check whether if updated value is valid or not
            updatedQuantity = orderDetailToUpdate.Quantity;
            if (updatedQuantity < 1)
            {
                throw new ArgumentException("Quantity is not valid");
            }

            //Save changes
            //Begin transaction
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.SaveChangesAsync();
                if(updatedQuantity > currentQuantity)
                {
                    product.UnitsInStock = (short?)(product.UnitsInStock - (updatedQuantity - currentQuantity));
                    product.UnitsOnOrder = (short?)(product.UnitsOnOrder + (updatedQuantity - currentQuantity));
                }
                else if (updatedQuantity < currentQuantity)
                {
                    product.UnitsInStock = (short?)(product.UnitsInStock + (currentQuantity - updatedQuantity));
                    product.UnitsOnOrder = (short?)(product.UnitsOnOrder - (currentQuantity - updatedQuantity));
                }
                else
                {
                    return orderDetailToUpdate;
                }
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (SqlException ex)
            {
                throw ex;
            }

            return orderDetailToUpdate;
        }
    }
}
