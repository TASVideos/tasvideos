using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TASVideos.Services
{
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
}
