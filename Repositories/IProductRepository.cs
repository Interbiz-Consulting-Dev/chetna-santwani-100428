using ECommerceProductManager.Models;

namespace ECommerceProductManager.Repositories;

public interface IProductRepository
{
    /// <summary>Returns all products currently stored in the repository.</summary>
    IReadOnlyList<Product> GetAll();

    /// <summary>Finds a product by its ID, or returns null when it does not exist.</summary>
    /// <param name="id">ID of the product to find.</param>
    /// <returns>The matching product, or null.</returns>
    Product? GetById(int id);

    /// <summary>Adds a product to the repository.</summary>
    /// <param name="product">Product to store.</param>
    void Add(Product product);

    /// <summary>Removes a product by ID.</summary>
    /// <param name="id">ID of the product to remove.</param>
    /// <returns>True if a product was removed; otherwise, false.</returns>
    bool Delete(int id);
}