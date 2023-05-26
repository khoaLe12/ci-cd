using Microsoft.EntityFrameworkCore;
using WebService.Model;

namespace WebService.Services;

public class ProductsRepository : IProductsRepository
{
    private readonly NorthwindContext context;

    public ProductsRepository(NorthwindContext context)
    {
        this.context = context;
    }

    public IEnumerable<Product> GetAllProducts() => context.Products.AsNoTracking().Where(p => !p.Discontinued).ToArray();

    public Product? GetProductById(int? id) => context.Products.Where(p => !p.Discontinued).FirstOrDefault(p => p.ProductId == id);

    public Product AddProduct(Product newProduct)
    {
        context.Add(newProduct);
        context.SaveChanges();
        return newProduct;
    }

    public async Task RemoveProduct(int? id)
    {
        var productToDelete = GetProductById(id);
        if (productToDelete == null)
        {
            throw new ArgumentException("No product exists with the given id", nameof(id));
        }
        productToDelete.Discontinued = true;
        await context.SaveChangesAsync();
    }

    public async Task<Product> UpdateProduct(int id, Product updatedProduct)
    {
        var existedProduct = GetProductById(id);
        if (existedProduct == null)
        {
            throw new ArgumentException("No product exists with the given id", nameof(id));
        }
        else
        {
            existedProduct.ProductName = updatedProduct.ProductName;
            existedProduct.QuantityPerUnit = updatedProduct.QuantityPerUnit;
            existedProduct.UnitPrice = updatedProduct.UnitPrice;
            existedProduct.UnitsInStock = updatedProduct.UnitsInStock;
            existedProduct.UnitsOnOrder = updatedProduct.UnitsOnOrder;
            existedProduct.ReorderLevel = updatedProduct.ReorderLevel;
            await context.SaveChangesAsync();
            return existedProduct;
        }
    }
}
