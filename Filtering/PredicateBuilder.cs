using System.Linq.Expressions;

namespace ECommerceProductManager.Filtering;

public static class PredicateBuilder
{
    /// <summary>Combines two predicates with a short-circuiting logical AND.</summary>
    /// <typeparam name="T">Type evaluated by both predicates.</typeparam>
    /// <param name="first">First condition to evaluate.</param>
    /// <param name="second">Second condition to evaluate.</param>
    /// <returns>A predicate that is true only when both conditions are true.</returns>
    public static Expression<Func<T, bool>> And<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(T), "item");
        var firstBody = new ParameterReplacer(first.Parameters[0], parameter).Visit(first.Body)!;
        var secondBody = new ParameterReplacer(second.Parameters[0], parameter).Visit(second.Body)!;
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(firstBody, secondBody), parameter);
    }

    private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        /// <summary>Replaces the original lambda parameter with the combined predicate's parameter.</summary>
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
    }
}