using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Providers
{
    public abstract class AsyncEnumerableQuery
    {
        internal abstract Expression Expression { get; }

        internal abstract IAsyncEnumerable Enumerable { get; }

        protected AsyncEnumerableQuery()
        {
        }
    }

    public class AsyncEnumerableQuery<T> : 
        AsyncEnumerableQuery,
        IOrderedQueryable<T>,
        IAsyncEnumerable<T>,
        IAsyncQueryProvider
    {
        private readonly Expression expression;

        private readonly IAsyncEnumerable<T> enumerable;

        private readonly EnumerableQuery<T> query;

        private readonly IQueryProvider underlyingProvider;

        public AsyncEnumerableQuery(
            IAsyncEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
            this.query = new EnumerableQuery<T>(enumerable);
            this.expression = Expression.Constant(this.query);
            this.underlyingProvider = this.query;
        }

        public AsyncEnumerableQuery(
            Expression expression)
        {
            this.expression = expression;
            this.query = new EnumerableQuery<T>(expression);
            this.underlyingProvider = this.query;
        }

        internal override IAsyncEnumerable Enumerable
        {
            get { return this.enumerable; }
        }

        internal override Expression Expression
        {
            get { return this.expression; }
        }

        Expression IQueryable.Expression
        {
            get { return this.expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this; }
        }

        public override string ToString()
        {
            var constantExpression = this.expression as ConstantExpression;
            if (constantExpression == null || constantExpression.Value != this)
            {
                return this.expression.ToString();
            }
            if (this.enumerable != null)
            {
                return this.enumerable.ToString();
            }

            return "null";
        }

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InternalGetAsyncEnumerator();
        }

        IQueryable IQueryProvider.CreateQuery(
            Expression expression)
        {
            Check.NotNull(expression, "expression");

            var result = this.underlyingProvider.CreateQuery(expression);

            if (result is IAsyncEnumerable)
            {
                return result;
            }

            var constantExpression = Expression.Constant(result);
            var createQuery = typeof(IQueryProvider)
                .GetMethods()
                .Single(m => m.Name == "CreateQuery" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(result.ElementType);
            
            return createQuery.Invoke(this, new[] { constantExpression }) as IQueryable;
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(
            Expression expression)
        {
            Check.NotNull(expression, "expression");

            var result = this.underlyingProvider.CreateQuery<S>(expression);

            if (result is IAsyncEnumerable<S>)
            {
                return result;
            }

            if (result is IAsyncEnumerable)
            {
                return result.Cast<S>();
            }
            
            return new AsyncEnumerableQuery<S>(Expression.Constant(result));
        }

        object IQueryProvider.Execute(
            Expression expression)
        {
            Check.NotNull(expression, "expression");

            return this.underlyingProvider.Execute(expression);
        }

        S IQueryProvider.Execute<S>(
            Expression expression)
        {
            Check.NotNull(expression, "expression");

            return this.underlyingProvider.Execute<S>(expression);
        }

        Task<object> IAsyncQueryProvider.ExecuteAsync(
            Expression expression, 
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Check.NotNull(expression, "expression");

            return Task.Run(() => this.underlyingProvider.Execute(expression), cancellationToken);
        }

        Task<S> IAsyncQueryProvider.ExecuteAsync<S>(
            Expression expression, 
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Check.NotNull(expression, "expression");

            return Task.Run(() => this.underlyingProvider.Execute<S>(expression), cancellationToken);
        }

        private IAsyncEnumerator<T> InternalGetAsyncEnumerator()
        {
            if (this.enumerable == null)
            {
                return this.query.AsAsyncEnumerable().GetAsyncEnumerator();
            }

            return this.enumerable.GetAsyncEnumerator();
        }
    }
}
