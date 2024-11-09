using System.Linq.Expressions;

namespace TASVideos.Extensions;

public static class EnumerableExtensions
{
	/// <summary>
	/// Returns the first half of the given collection,
	/// in the case of odd numbers, the odd item is considered in the first half.
	/// </summary>
	public static IEnumerable<T> FirstHalf<T>(this IEnumerable<T> source)
	{
		var list = source.ToList();
		int half = (int)Math.Ceiling(list.Count / 2.0);
		return list.Take(half);
	}

	/// <summary>
	/// Returns the second half of the given collection,
	/// in the case of odd numbers, the odd item is considered in the first half.
	/// </summary>
	public static IEnumerable<T> SecondHalf<T>(this IEnumerable<T> source)
	{
		var list = source.ToList();
		int half = (int)Math.Ceiling(list.Count / 2.0);
		return list.Skip(half);
	}

	/// <summary>
	/// Returns a random entry from the given collection.
	/// </summary>
	public static T AtRandom<T>(this ICollection<T> collection)
	{
		var randomIndex = new Random().Next(0, collection.Count);
		return collection.ElementAt(randomIndex);
	}

	/// <summary>
	/// Adds the elements of the given collection
	/// </summary>
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newValues)
	{
		foreach (var value in newValues)
		{
			collection.Add(value);
		}
	}

	/// <summary>
	/// An overload of OrderBy that will call OrderByDescending depending on the <see cref="descending"/> param
	/// </summary>
	public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool descending)
	{
		return descending
			? source.OrderByDescending(keySelector)
			: source.OrderBy(keySelector);
	}
}
