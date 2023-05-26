using WebService.Model;

namespace WebService.Services;

public interface IProductsRepository
{
    IEnumerable<Product> GetAllProducts();
    Product? GetProductById(int? id);
    Product AddProduct(Product newProduct);
    Task RemoveProduct(int? id);
    Task<Product> UpdateProduct(int id, Product updatedProduct);
}
