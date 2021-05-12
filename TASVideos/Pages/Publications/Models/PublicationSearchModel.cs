using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.RazorPages.Pages.Publications.Models
{
	public class PublicationSearchModel : IPublicationTokens
	{
		public IEnumerable<string> SystemCodes { get; set; } = new List<string>();
		public IEnumerable<string> Tiers { get; set; } = new List<string>();
		public IEnumerable<int> Years { get; set; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1);
		public IEnumerable<string> Tags { get; set; } = new List<string>();
		public IEnumerable<string> Genres { get; set; } = new List<string>();
		public IEnumerable<string> Flags { get; set; } = new List<string>();

		public bool ShowObsoleted { get; set; }

		public IEnumerable<int> Authors { get; set; } = new List<int>();

		public IEnumerable<int> MovieIds { get; set; } = new List<int>();

		public IEnumerable<int> Games { get; set; } = new List<int>();
		public IEnumerable<int> GameGroups { get; set; } = new List<int>();

		public bool IsEmpty => !SystemCodes.Any()
			&& !Tiers.Any()
			&& !Years.Any()
			&& !Flags.Any()
			&& !Tags.Any()
			&& !Genres.Any()
			&& !Authors.Any()
			&& !MovieIds.Any()
			&& !Games.Any()
			&& !GameGroups.Any();
	}
}
