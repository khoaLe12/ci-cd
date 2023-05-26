using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebService.Controllers;
using WebService.Model;
using WebService.Services;

namespace WebService.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.True(1 == 1);
    }

    [Fact]
    public void ProductIntegrationTest()
    {
        // Create DBContext
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
        optionsBuilder.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);
        var context = new NorthwindContext(optionsBuilder.Options);

        //Create Database used for testing
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Create an instance of repository
        var repository = new ProductsRepository(context);

        // Create Controller
        var controller = new ProductsController(repository, context);

        // Add Product
        controller.AddProduct(
            new Product()
            {
                ProductName = "San Pham Moi",
                QuantityPerUnit = "10 boxes x 20 bags",
                UnitPrice = 18,
                UnitsInStock = 29,
                UnitsOnOrder = 10,
                ReorderLevel = 10,
                Discontinued = false
            });

        //Test after adding, there is one element
        var actionResult = controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value).ToArray();
        Assert.Single(products);
        Assert.Equal("San Pham Moi", products[0].ProductName);
    }
}