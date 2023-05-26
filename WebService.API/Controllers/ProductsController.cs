using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebService.Model;
using WebService.Services;

namespace WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : Controller
{
    private readonly NorthwindContext context;
    private readonly IProductsRepository repository;

    public ProductsController(IProductsRepository repository, NorthwindContext context)
    {
        this.context = context;
        this.repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
    public IActionResult GetAll() => Ok(repository.GetAllProducts());


    [HttpGet("{id}", Name = nameof(GetProductByID))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProductByID(int? id)
    {
        var existingProduct = repository.GetProductById(id);
        if (existingProduct == null) return NotFound();
        return Ok(existingProduct);
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
    public IActionResult AddProduct([FromBody] Product newProduct)
    {
        newProduct.ProductId = 0;
        repository.AddProduct(newProduct);
        return CreatedAtAction(nameof(GetProductByID), new {id = newProduct.ProductId}, newProduct);
    }


    [HttpDelete]
    [Route("{idToDelete}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveProduct(int? idToDelete)
    {
        try
        {
            await repository.RemoveProduct(idToDelete);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        return NoContent();
    }


    [HttpPut]
    [Route("{idToUpdate}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int idToUpdate, [FromBody] Product updatedProduct)
    {
        try
        {
            updatedProduct = await repository.UpdateProduct(idToUpdate, updatedProduct);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        return Ok(updatedProduct);
    }
}
