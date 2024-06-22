using System.Collections.Immutable;

namespace TASVideos.Common;

public class ListDiff(IReadOnlyCollection<string> currentItems, ICollection<string> newItems)
{
	public IReadOnlyCollection<string> Added => newItems.Except(currentItems).ToImmutableArray();
	public IReadOnlyCollection<string> Removed => currentItems.Except(newItems).ToImmutableList();
}

public static class ListDiffExtensions
{
	public static IEnumerable<string> ToMessages(this ListDiff diff, string name)
	{
		if (diff.Added.Any())
		{
			yield return $"Added {name}: {string.Join(", ", diff.Added.OrderBy(s => s))}";
		}

		if (diff.Removed.Any())
		{
			yield return $"Removed {name}: {string.Join(", ", diff.Removed.OrderBy(s => s))}";
		}
	}
}
