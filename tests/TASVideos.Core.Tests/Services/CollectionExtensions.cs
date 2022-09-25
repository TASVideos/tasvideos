using System.Collections;
using System.Linq.Expressions;

namespace TASVideos.Core.Tests.Services;

public static class CollectionExtensions
{
	public static IQueryable<T> AsAsyncQueryable<T>(this ICollection<T> source) =>
		new AsyncQueryable<T>(source.AsQueryable());
}

internal class AsyncQueryable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
	private readonly IQueryable<T> _source;

	public AsyncQueryable(IQueryable<T> source)
	{
		_source = source;
	}

	public Type ElementType => typeof(T);

	public Expression Expression => _source.Expression;

	public IQueryProvider Provider => new AsyncQueryProvider<T>(_source.Provider);

	public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		return new AsyncEnumeratorWrapper<T>(_source.GetEnumerator());
	}

	public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class AsyncQueryProvider<T> : IQueryProvider
{
	private readonly IQueryProvider _source;

	public AsyncQueryProvider(IQueryProvider source)
	{
		_source = source;
	}

	public IQueryable CreateQuery(Expression expression) =>
		_source.CreateQuery(expression);

	public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
		new AsyncQueryable<TElement>(_source.CreateQuery<TElement>(expression));

	public object Execute(Expression expression) => Execute<T>(expression);

	public TResult Execute<TResult>(Expression expression) =>
		_source.Execute<TResult>(expression);
}

internal class AsyncEnumeratorWrapper<T> : IAsyncEnumerator<T>
{
	private readonly IEnumerator<T> _source;

	public AsyncEnumeratorWrapper(IEnumerator<T> source)
	{
		_source = source;
	}

	public T Current => _source.Current;

	public ValueTask DisposeAsync()
	{
		return new ValueTask(Task.CompletedTask);
	}

	public ValueTask<bool> MoveNextAsync()
	{
		return new ValueTask<bool>(_source.MoveNext());
	}
}
