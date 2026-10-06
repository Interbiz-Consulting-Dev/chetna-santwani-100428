using System.Linq.Expressions;
using ECommerceProductManager.Models;

namespace ECommerceProductManager.Filtering;

public enum FilterField
{
    Category = 1,
    Brand,
    Price,
    Rating,
    Stock
}

public enum FilterOperator
{
    Equals = 1,
    Contains,
    GreaterThan,
    LessThan,
    Between
}

public abstract class ProductFilterCondition
{
    /// <summary>Builds the expression tree that tests whether a product matches this condition.</summary>
    public abstract Expression<Func<Product, bool>> ToExpression();
}

public sealed class TextProductFilterCondition : ProductFilterCondition
{
    private readonly FilterField _field;
    private readonly FilterOperator _operator;
    private readonly string _value;

    /// <summary>Creates a category or brand condition using equals or contains matching.</summary>
    /// <param name="field">Text product field to compare.</param>
    /// <param name="filterOperator">Text comparison to perform.</param>
    /// <param name="value">Text value to match, ignoring letter case.</param>
    public TextProductFilterCondition(FilterField field, FilterOperator filterOperator, string value)
    {
        if (field is not (FilterField.Category or FilterField.Brand))
        {
            throw new ArgumentException("Text filters support category and brand.", nameof(field));
        }

        if (filterOperator is not (FilterOperator.Equals or FilterOperator.Contains))
        {
            throw new ArgumentException("Text filters support equals and contains.", nameof(filterOperator));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Filter value cannot be empty.", nameof(value));
        }

        _field = field;
        _operator = filterOperator;
        _value = value.Trim();
    }

    /// <summary>Builds a case-insensitive expression for the configured text comparison.</summary>
    /// <returns>An expression that returns true when a product matches this condition.</returns>
    public override Expression<Func<Product, bool>> ToExpression()
    {
        var product = Expression.Parameter(typeof(Product), "product");
        var property = Expression.Property(product, _field.ToString());
        var value = Expression.Constant(_value);
        var comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);

        Expression body = _operator == FilterOperator.Equals
            ? Expression.Call(typeof(string), nameof(string.Equals), Type.EmptyTypes, property, value, comparison)
            : Expression.GreaterThanOrEqual(
                Expression.Call(property, nameof(string.IndexOf), Type.EmptyTypes, value, comparison),
                Expression.Constant(0));

        return Expression.Lambda<Func<Product, bool>>(body, product);
    }
}

public sealed class NumericProductFilterCondition : ProductFilterCondition
{
    private readonly FilterField _field;
    private readonly FilterOperator _operator;
    private readonly decimal _minimum;
    private readonly decimal? _maximum;

    /// <summary>Creates a numeric condition for price, rating, or stock.</summary>
    /// <param name="field">Numeric product field to compare.</param>
    /// <param name="filterOperator">Numeric comparison to perform.</param>
    /// <param name="minimum">Comparison value or inclusive lower bound.</param>
    /// <param name="maximum">Inclusive upper bound when using the between operator.</param>
    public NumericProductFilterCondition(FilterField field, FilterOperator filterOperator, decimal minimum, decimal? maximum = null)
    {
        if (field is not (FilterField.Price or FilterField.Rating or FilterField.Stock))
        {
            throw new ArgumentException("Numeric filters support price, rating, and stock.", nameof(field));
        }

        if (filterOperator is not (FilterOperator.Equals or FilterOperator.GreaterThan or FilterOperator.LessThan or FilterOperator.Between))
        {
            throw new ArgumentException("Invalid numeric filter operator.", nameof(filterOperator));
        }

        if (filterOperator == FilterOperator.Between && maximum is null)
        {
            throw new ArgumentException("A maximum value is required for a between filter.", nameof(maximum));
        }

        if (maximum is not null && minimum > maximum.Value)
        {
            throw new ArgumentException("The minimum value cannot exceed the maximum value.", nameof(maximum));
        }

        _field = field;
        _operator = filterOperator;
        _minimum = minimum;
        _maximum = maximum;
    }

    /// <summary>Builds the expression tree for the configured numeric comparison.</summary>
    /// <returns>An expression that returns true when a product matches this condition.</returns>
    public override Expression<Func<Product, bool>> ToExpression()
    {
        var product = Expression.Parameter(typeof(Product), "product");
        var property = Expression.Convert(Expression.Property(product, _field.ToString()), typeof(decimal));
        var minimum = Expression.Constant(_minimum);

        Expression body = _operator switch
        {
            FilterOperator.Equals => Expression.Equal(property, minimum),
            FilterOperator.GreaterThan => Expression.GreaterThan(property, minimum),
            FilterOperator.LessThan => Expression.LessThan(property, minimum),
            FilterOperator.Between => Expression.AndAlso(
                Expression.GreaterThanOrEqual(property, minimum),
                Expression.LessThanOrEqual(property, Expression.Constant(_maximum!.Value))),
            _ => throw new InvalidOperationException("Unsupported numeric filter operator.")
        };

        return Expression.Lambda<Func<Product, bool>>(body, product);
    }
}