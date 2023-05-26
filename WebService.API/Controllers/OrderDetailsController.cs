using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using WebService.Model;
using WebService.Services;

namespace WebService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderDetailsController : ControllerBase
{
    private readonly NorthwindContext context;
    private readonly IOrderDetailsRepository orderDetailsRepository;

    public OrderDetailsController(IOrderDetailsRepository orderDetailsRepository, NorthwindContext context)
    {
        this.orderDetailsRepository = orderDetailsRepository;
        this.context = context;
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDetail))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrderDetailByID([FromQuery] int? orderID, [FromQuery] int? productID)
    {
        var existingOrderDetail = orderDetailsRepository.GetOrderDetailByID(orderID, productID);
        if (existingOrderDetail == null) return NotFound();
        return Ok(existingOrderDetail);
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDetail))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> AddProductToOrder([FromBody] OrderDetail newOrderDetail)
    {
        if (newOrderDetail.OrderId == null || newOrderDetail.ProductId == null)
        {
            return BadRequest("Order Id and Product Id are required.");
        }
        else if (newOrderDetail.Quantity < 1)
        {
            return BadRequest("Quantity is not valid.");
        }
        else if (orderDetailsRepository.GetOrderDetailByID(newOrderDetail.OrderId, newOrderDetail.ProductId) != null)
        {
            return Conflict("Order Detail already exist.");
        }
        else
        {
            try
            {
                var orderDetail = await orderDetailsRepository.AddProductToOrder(newOrderDetail);
                if (orderDetail == null)
                {
                    return NotFound("Order or Product Not Found !!!");
                }
                else
                {
                    return CreatedAtAction(
                        nameof(GetOrderDetailByID),
                        new
                        {
                            orderID = orderDetail.OrderId,
                            productID = orderDetail.ProductId
                        },
                        orderDetail
                        );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
            }
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> RemoveOrderDetail([FromQuery] int? orderId, [FromQuery] int? productId)
    {
        if (orderId == null || productId == null)
            return BadRequest("Order Id and Product Id are required.");
        try
        {
            await orderDetailsRepository.RemoveOrderDetail((int)orderId, (int)productId);
        }
        catch (Exception ex)
        {
            if(ex is ArgumentException)
            {
                return NotFound(ex.ToString());
            }   
            if(ex is SqlException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error while save changes: " + ex.ToString());
            }
        }
        return NoContent();
    }

    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDetail))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> UpdateOrderDetail([FromQuery] int orderId, [FromQuery] int productId, [FromBody] JsonPatchDocument<OrderDetail> patchDoc)
    {
        if(patchDoc == null)  return BadRequest();
        try
        {
            var orderDetail = await orderDetailsRepository.UpdateOrderDetail(orderId, productId, patchDoc, ModelState);
            if (orderDetail == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(orderDetail);
            }
        }
        catch (Exception ex)
        {
            if(ex is SqlException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error while save changes: " + ex.ToString());
            }
            return BadRequest(ex.ToString());
        }
    }
}
