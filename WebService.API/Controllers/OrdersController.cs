using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebService.Model;
using WebService.Services;

namespace WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : Controller
{
    private readonly NorthwindContext context;
    private readonly IOrdersRepository repository;

    public OrdersController(IOrdersRepository repository, NorthwindContext context)
    {
        this.repository = repository;
        this.context = context;
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Order>))]
    public IActionResult GetAllOrders() => Ok(repository.GetAllOrders());


    [HttpGet("{id}", Name = nameof(GetOrderByID))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Order))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrderByID(int? id) 
    {
        var existingOrder = repository.GetOrderByID(id);
        if (existingOrder == null) return NotFound();
        return Ok(existingOrder);
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Order))]
    public IActionResult CreateNewOrder([FromBody] Order newOrder)
    {
        repository.CreateNewOrder(newOrder);
        return CreatedAtAction(nameof(GetOrderByID), new { id = newOrder.OrderId }, newOrder);
    }

    [HttpPut]
    [Route("{idToUpdate}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Order))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(int idToUpdate, [FromBody] Order updatedOrder)
    {
        try
        {
            updatedOrder = await repository.UpdateOrder(idToUpdate, updatedOrder);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        return Ok(updatedOrder);
    }

    [HttpDelete]
    [Route("{idToDelete}")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> RemoveOrder(int? idToDelete)
    {
        try
        {
            await repository.RemoveOrder(idToDelete);
        }
        catch (Exception ex)
        {
            if (ex is SqlException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error while saving changes: " + ex.ToString());
            }
            if (ex is ArgumentException)
            {
                return NotFound(ex.ToString());
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error: " + ex.ToString());
            }
        }
        return NoContent();
    }
}
