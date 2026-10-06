namespace ECommerceProductManager.Models;

public sealed class Product
{
    public int Id { get; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal Rating { get; private set; }
    public int Stock { get; private set; }

    /// <summary>Creates a product after validating its ID and details.</summary>
    /// <param name="id">Unique positive product identifier.</param>
    /// <param name="name">Product name.</param>
    /// <param name="category">Product category.</param>
    /// <param name="brand">Product brand.</param>
    /// <param name="price">Non-negative product price.</param>
    /// <param name="rating">Product rating from 0 to 5.</param>
    /// <param name="stock">Non-negative quantity in stock.</param>
    public Product(int id, string name, string category, string brand, decimal price, decimal rating, int stock)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Product ID must be greater than zero.");
        }

        Id = id;
        UpdateDetails(name, category, brand, price, rating, stock);
    }

    /// <summary>Validates and replaces this product's editable details.</summary>
    /// <param name="name">Product name.</param>
    /// <param name="category">Product category.</param>
    /// <param name="brand">Product brand.</param>
    /// <param name="price">Non-negative product price.</param>
    /// <param name="rating">Product rating from 0 to 5.</param>
    /// <param name="stock">Non-negative quantity in stock.</param>
    public void UpdateDetails(string name, string category, string brand, decimal price, decimal rating, int stock)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(brand))
        {
            throw new ArgumentException("Name, category, and brand are required.");
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        if (rating is < 0 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5.");
        }

        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");
        }

        Name = name.Trim();
        Category = category.Trim();
        Brand = brand.Trim();
        Price = price;
        Rating = rating;
        Stock = stock;
    }
}