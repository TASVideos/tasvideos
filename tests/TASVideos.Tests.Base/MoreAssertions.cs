using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TASVideos.Tests.Base;

public static class MoreAssertions
{
	public static void AreSameLength<T1, T2>(
		this Assert assert,
		IEnumerable<T1> first,
		IEnumerable<T2> second,
		string? message = null)
	{
		message ??= "Collections aren't the same length";
		if (first.TryGetNonEnumeratedCount(out var firstCount))
		{
			if (second.TryGetNonEnumeratedCount(out var secondCount))
			{
				Assert.AreEqual(firstCount, secondCount, message);
			}
			else
			{
				if (!OptimisedHasCount(firstCount, second))
				{
					Assert.Fail(message);
				}
			}
		}
		else
		{
			if (second.TryGetNonEnumeratedCount(out var secondCount))
			{
				if (!OptimisedHasCount(secondCount, first))
				{
					Assert.Fail(message);
				}
			}
			else
			{
				if (!OptimisedHaveSameCount(first, second))
				{
					Assert.Fail(message);
				}
			}
		}
	}

	private static bool OptimisedHasCount<T>(int count, IEnumerable<T> collection)
	{
		using var iter = collection.GetEnumerator();
		for (var i = 0; i < count; i++)
		{
			if (!iter.MoveNext())
			{
				return false;
			}
		}

		// should be at end
		return !iter.MoveNext();
	}

	private static bool OptimisedHaveSameCount<T1, T2>(IEnumerable<T1> collection, IEnumerable<T2> collection1)
	{
		using var iter = collection.GetEnumerator();
		using var iter1 = collection1.GetEnumerator();
		while (iter.MoveNext())
		{
			if (!iter1.MoveNext())
			{
				return false;
			}
		}

		// at end of first, so should be at end of second
		return !iter1.MoveNext();
	}
}
