using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Models
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

		public bool IsEmpty => (SystemCodes == null || !SystemCodes.Any())
			&& (Tiers == null || !Tiers.Any())
			&& (Years == null || !Years.Any())
			&& (Flags == null || !Flags.Any())
			&& (Tags == null || !Tags.Any())
			&& (Genres == null || !Genres.Any())
			&& (Authors == null || !Authors.Any())
			&& (MovieIds == null || !MovieIds.Any());
	}

}
