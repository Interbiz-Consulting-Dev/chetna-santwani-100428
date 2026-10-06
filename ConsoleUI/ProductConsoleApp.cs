using System.Globalization;
using ECommerceProductManager.Filtering;
using ECommerceProductManager.Models;
using ECommerceProductManager.Services;

namespace ECommerceProductManager.ConsoleUI;

public sealed class ProductConsoleApp(ProductCatalogService catalog)
{
    /// <summary>Runs the main product-management menu until the user exits or input ends.</summary>
    public void Run()
    {
        try
        {
            while (true)
            {
                Console.WriteLine("\n=== E-Commerce Product Manager ===");
                Console.WriteLine("1. View, filter, sort, and page products");
                Console.WriteLine("2. Search products");
                Console.WriteLine("3. Add product");
                Console.WriteLine("4. Update product");
                Console.WriteLine("5. Delete product");
                Console.WriteLine("6. Exit");

                switch (ReadInt("Select an option: ", 1, 6))
                {
                    case 1:
                        RunSafely(BrowseProducts);
                        break;
                    case 2:
                        RunSafely(SearchProducts);
                        break;
                    case 3:
                        RunSafely(AddProduct);
                        break;
                    case 4:
                        RunSafely(UpdateProduct);
                        break;
                    case 5:
                        RunSafely(DeleteProduct);
                        break;
                    case 6:
                        return;
                }
            }
        }
        catch (EndOfStreamException)
        {
        }
    }

    /// <summary>Collects optional filters, sorting, and pagination choices, then displays the results.</summary>
    private void BrowseProducts()
    {
        var conditions = new List<ProductFilterCondition>();
        while (ReadYesNo("Add a filter condition? (y/n): "))
        {
            conditions.Add(ReadFilterCondition());
        }

        IReadOnlyList<Product> results = conditions.Count == 0 ? catalog.GetAll() : catalog.Filter(conditions);
        if (results.Count == 0)
        {
            Console.WriteLine("No products match those filters.");
            return;
        }

        if (ReadYesNo("Sort these products? (y/n): "))
        {
            Console.WriteLine("Sort by: 1. Price  2. Rating  3. Product name");
            var field = (ProductSortField)ReadInt("Select a field: ", 1, 3);
            var descending = ReadInt("Order: 1. Ascending  2. Descending: ", 1, 2) == 2;
            results = catalog.Sort(results, field, descending);
        }

        var pageSize = ReadInt("Products per page: ", 1, int.MaxValue);
        var pageCount = (int)Math.Ceiling(results.Count / (double)pageSize);
        var pageNumber = ReadInt($"Page number (1-{pageCount}): ", 1, pageCount);
        DisplayProducts(catalog.GetPage(results, pageNumber, pageSize));
        Console.WriteLine($"Page {pageNumber} of {pageCount} ({results.Count} matching product(s)).");
    }

    /// <summary>Reads a filter field, comparison, and value from the console.</summary>
    /// <returns>The filter condition selected by the user.</returns>
    private ProductFilterCondition ReadFilterCondition()
    {
        Console.WriteLine("Filter by: 1. Category  2. Brand  3. Price  4. Rating  5. Stock");
        var field = (FilterField)ReadInt("Select a field: ", 1, 5);

        if (field is FilterField.Category or FilterField.Brand)
        {
            Console.WriteLine("Match: 1. Equals  2. Contains");
            var filterOperator = ReadInt("Select a match: ", 1, 2) == 1
                ? FilterOperator.Equals
                : FilterOperator.Contains;
            return new TextProductFilterCondition(field, filterOperator, ReadRequiredText("Enter a value: "));
        }

        Console.WriteLine("Compare: 1. Equals  2. Greater than  3. Less than  4. Between");
        var numericOperator = ReadInt("Select a comparison: ", 1, 4) switch
        {
            1 => FilterOperator.Equals,
            2 => FilterOperator.GreaterThan,
            3 => FilterOperator.LessThan,
            _ => FilterOperator.Between
        };
        var minimum = ReadDecimal("Enter a value or minimum: ");
        decimal? maximum = numericOperator == FilterOperator.Between ? ReadDecimal("Enter maximum: ") : null;
        return new NumericProductFilterCondition(field, numericOperator, minimum, maximum);
    }

    /// <summary>Searches products by ID or name and displays matching results.</summary>
    private void SearchProducts()
    {
        var term = ReadRequiredText("Enter a product ID or name: ");
        var results = catalog.Search(term);
        DisplayProducts(results);
        Console.WriteLine($"{results.Count} product(s) found.");
    }

    /// <summary>Reads product details and adds a new product to the catalog.</summary>
    private void AddProduct()
    {
        var product = ReadProductDetails();
        var added = catalog.Add(product.Name, product.Category, product.Brand, product.Price, product.Rating, product.Stock);
        Console.WriteLine($"Added product with ID {added.Id}.");
    }

    /// <summary>Reads an existing product ID and replaces its editable details.</summary>
    private void UpdateProduct()
    {
        var id = ReadInt("Product ID to update: ", 1, int.MaxValue);
        if (catalog.GetById(id) is null)
        {
            Console.WriteLine("Product not found.");
            return;
        }

        var details = ReadProductDetails();
        Console.WriteLine(catalog.Update(id, details.Name, details.Category, details.Brand, details.Price, details.Rating, details.Stock)
            ? "Product updated."
            : "Product not found.");
    }

    /// <summary>Reads a product ID and removes that product from the catalog.</summary>
    private void DeleteProduct()
    {
        var id = ReadInt("Product ID to delete: ", 1, int.MaxValue);
        Console.WriteLine(catalog.Delete(id) ? "Product deleted." : "Product not found.");
    }

    /// <summary>Prompts for and returns all editable product details.</summary>
    /// <returns>The entered name, category, brand, price, rating, and stock.</returns>
    private static (string Name, string Category, string Brand, decimal Price, decimal Rating, int Stock) ReadProductDetails()
    {
        var name = ReadRequiredText("Name: ");
        var category = ReadRequiredText("Category: ");
        var brand = ReadRequiredText("Brand: ");
        var price = ReadDecimal("Price: ");
        var rating = ReadDecimal("Rating (0-5): ");
        var stock = ReadInt("Stock: ", 0, int.MaxValue);
        return (name, category, brand, price, rating, stock);
    }

    /// <summary>Prints products in aligned columns in the console.</summary>
    /// <param name="products">Products to display.</param>
    private static void DisplayProducts(IEnumerable<Product> products)
    {
        Console.WriteLine();
        Console.WriteLine($"{"ID",-5} {"Name",-24} {"Category",-18} {"Brand",-16} {"Price",10} {"Rating",8} {"Stock",7}");
        Console.WriteLine(new string('-', 94));
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Id,-5} {Truncate(product.Name, 24),-24} {Truncate(product.Category, 18),-18} {Truncate(product.Brand, 16),-16} {product.Price,10:F2} {product.Rating,8:F1} {product.Stock,7}");
        }
    }

    /// <summary>Shortens text to fit a table column, adding an ellipsis when needed.</summary>
    /// <param name="value">Text to shorten.</param>
    /// <param name="length">Maximum output length.</param>
    /// <returns>The original or shortened text.</returns>
    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..(length - 3)] + "...";

    /// <summary>Reads an integer within the inclusive range, repeating the prompt for invalid input.</summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <param name="minimum">Smallest accepted value.</param>
    /// <param name="maximum">Largest accepted value.</param>
    /// <returns>The validated integer entered by the user.</returns>
    private static int ReadInt(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(ReadInputLine(), out var value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Enter a whole number from {minimum} to {maximum}.");
        }
    }

    /// <summary>Reads a decimal number, repeating the prompt for invalid input.</summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <returns>The decimal value entered by the user.</returns>
    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(ReadInputLine(), NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
            {
                return value;
            }

            Console.WriteLine("Enter a valid number.");
        }
    }

    /// <summary>Reads non-empty text, repeating the prompt when input is blank.</summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <returns>The trimmed text entered by the user.</returns>
    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = ReadInputLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("A value is required.");
        }
    }

    /// <summary>Reads a yes-or-no answer, accepting y or n regardless of letter case.</summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <returns>True for yes; false for no.</returns>
    private static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var answer = ReadInputLine().Trim();
            if (string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(answer, "n", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Enter y or n.");
        }
    }

    /// <summary>Runs a menu action and displays expected operation errors without terminating the menu.</summary>
    /// <param name="operation">Action to run.</param>
    private static void RunSafely(Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            Console.WriteLine($"Operation failed: {exception.Message}");
        }
    }

    /// <summary>Reads one console line or signals that the input stream has ended.</summary>
    /// <returns>The line entered by the user.</returns>
    private static string ReadInputLine() => Console.ReadLine() ?? throw new EndOfStreamException();
}