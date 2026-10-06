using ECommerceProductManager.Models;

namespace ECommerceProductManager.Repositories;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    /// <summary>Creates a repository initialized with the supplied products.</summary>
    /// <param name="products">Initial products to store.</param>
    public InMemoryProductRepository(IEnumerable<Product> products)
    {
        _products = products.ToList();
    }

    /// <summary>Returns a read-only view of all stored products.</summary>
    public IReadOnlyList<Product> GetAll() => _products.AsReadOnly();

    /// <summary>Finds a product by ID, or returns null when it is not stored.</summary>
    /// <param name="id">ID of the product to find.</param>
    public Product? GetById(int id) => _products.SingleOrDefault(product => product.Id == id);

    /// <summary>Adds a product, rejecting duplicate IDs.</summary>
    /// <param name="product">Product to add.</param>
    public void Add(Product product)
    {
        if (_products.Any(existing => existing.Id == product.Id))
        {
            throw new InvalidOperationException($"Product ID {product.Id} already exists.");
        }

        _products.Add(product);
    }

    /// <summary>Removes the product with the specified ID when present.</summary>
    /// <param name="id">ID of the product to remove.</param>
    /// <returns>True if a product was removed; otherwise, false.</returns>
    public bool Delete(int id)
    {
        var product = GetById(id);
        return product is not null && _products.Remove(product);
    }
}