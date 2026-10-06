using System.Linq.Expressions;
using ECommerceProductManager.Filtering;
using ECommerceProductManager.Models;
using ECommerceProductManager.Repositories;

namespace ECommerceProductManager.Services;

public enum ProductSortField
{
    Price = 1,
    Rating,
    Name
}

public sealed class ProductCatalogService(IProductRepository repository)
{
    /// <summary>Returns every product in the catalog.</summary>
    public IReadOnlyList<Product> GetAll() => repository.GetAll();

    /// <summary>Finds a product by ID, or returns null when it does not exist.</summary>
    /// <param name="id">ID of the product to find.</param>
    public Product? GetById(int id) => repository.GetById(id);

    /// <summary>Searches product names, or looks up a product directly when the term is an ID.</summary>
    /// <param name="searchTerm">Product name text or numeric product ID.</param>
    /// <returns>Products whose names contain the term, or the matching ID result.</returns>
    public IReadOnlyList<Product> Search(string searchTerm)
    {
        if (int.TryParse(searchTerm, out var id))
        {
            var product = repository.GetById(id);
            return product is null ? [] : [product];
        }

        return repository.GetAll()
            .Where(product => product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Applies all supplied conditions to the catalog using a combined expression tree.</summary>
    /// <param name="conditions">Conditions that every returned product must satisfy.</param>
    /// <returns>Products matching every condition.</returns>
    public IReadOnlyList<Product> Filter(IEnumerable<ProductFilterCondition> conditions)
    {
        Expression<Func<Product, bool>> predicate = product => true;
        foreach (var condition in conditions)
        {
            predicate = PredicateBuilder.And(predicate, condition.ToExpression());
        }

        return repository.GetAll().AsQueryable().Where(predicate).ToList();
    }

    /// <summary>Sorts products by price, rating, or name in the requested direction.</summary>
    /// <param name="products">Products to sort.</param>
    /// <param name="field">Product property used as the sort key.</param>
    /// <param name="descending">True for descending order; otherwise, ascending.</param>
    /// <returns>A newly sorted product list.</returns>
    public IReadOnlyList<Product> Sort(IEnumerable<Product> products, ProductSortField field, bool descending)
    {
        Func<Product, object> keySelector = field switch
        {
            ProductSortField.Price => product => product.Price,
            ProductSortField.Rating => product => product.Rating,
            ProductSortField.Name => product => product.Name,
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        return descending
            ? products.OrderByDescending(keySelector).ToList()
            : products.OrderBy(keySelector).ToList();
    }

    /// <summary>Returns one 1-based page from a product sequence.</summary>
    /// <param name="products">Products to page.</param>
    /// <param name="pageNumber">Page number, starting at 1.</param>
    /// <param name="pageSize">Maximum number of products on the page.</param>
    /// <returns>Products on the requested page.</returns>
    public IReadOnlyList<Product> GetPage(IEnumerable<Product> products, int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number and page size must be greater than zero.");
        }

        return products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    }

    /// <summary>Creates and stores a product with the next available ID.</summary>
    /// <param name="name">Product name.</param>
    /// <param name="category">Product category.</param>
    /// <param name="brand">Product brand.</param>
    /// <param name="price">Product price.</param>
    /// <param name="rating">Product rating from 0 to 5.</param>
    /// <param name="stock">Quantity in stock.</param>
    /// <returns>The newly added product.</returns>
    public Product Add(string name, string category, string brand, decimal price, decimal rating, int stock)
    {
        var id = repository.GetAll().Select(product => product.Id).DefaultIfEmpty(0).Max() + 1;
        var product = new Product(id, name, category, brand, price, rating, stock);
        repository.Add(product);
        return product;
    }

    /// <summary>Updates the details of a product when its ID exists.</summary>
    /// <param name="id">ID of the product to update.</param>
    /// <param name="name">New product name.</param>
    /// <param name="category">New product category.</param>
    /// <param name="brand">New product brand.</param>
    /// <param name="price">New product price.</param>
    /// <param name="rating">New product rating from 0 to 5.</param>
    /// <param name="stock">New quantity in stock.</param>
    /// <returns>True if the product was found and updated; otherwise, false.</returns>
    public bool Update(int id, string name, string category, string brand, decimal price, decimal rating, int stock)
    {
        var product = repository.GetById(id);
        if (product is null)
        {
            return false;
        }

        product.UpdateDetails(name, category, brand, price, rating, stock);
        return true;
    }

    /// <summary>Deletes a product by ID.</summary>
    /// <param name="id">ID of the product to delete.</param>
    /// <returns>True if a product was deleted; otherwise, false.</returns>
    public bool Delete(int id) => repository.Delete(id);
}