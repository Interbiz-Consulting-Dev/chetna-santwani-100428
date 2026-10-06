using ECommerceProductManager.ConsoleUI;
using ECommerceProductManager.Models;
using ECommerceProductManager.Repositories;
using ECommerceProductManager.Services;

var products = new List<Product>
{
    new(1, "Galaxy S24", "Mobile", "Samsung", 79999, 4.6m, 18),
    new(2, "iPhone 15", "Mobile", "Apple", 74999, 4.7m, 12),
    new(3, "Pixel 8", "Mobile", "Google", 59999, 4.5m, 9),
    new(4, "Bravia 55-inch TV", "Television", "Sony", 68999, 4.4m, 7),
    new(5, "Inspiron 14", "Laptop", "Dell", 62999, 4.2m, 15),
    new(6, "IdeaPad Slim 5", "Laptop", "Lenovo", 57999, 4.3m, 11),
    new(7, "Noise Cancelling Headphones", "Audio", "Sony", 12999, 4.1m, 24),
    new(8, "Smart Band 8", "Wearable", "Xiaomi", 3499, 4.0m, 32)
};

var repository = new InMemoryProductRepository(products);
var catalog = new ProductCatalogService(repository);
new ProductConsoleApp(catalog).Run();