using System.Collections.Immutable;

namespace TASVideos.Core.Services;

public class ListDiff
{
	public ListDiff(ICollection<string> currentItems, ICollection<string> newItems)
	{
		Added = newItems.Except(currentItems).ToImmutableList();
		Removed = currentItems.Except(newItems).ToImmutableList();
	}

	public IReadOnlyCollection<string> Added { get; init; }
	public IReadOnlyCollection<string> Removed { get; init; }
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
