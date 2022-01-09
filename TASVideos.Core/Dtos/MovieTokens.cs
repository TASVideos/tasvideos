using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services
{
	public class MovieTokens : IPublicationTokens
	{
		public IEnumerable<string> SystemCodes { get; init; } = new List<string>();
		public IEnumerable<string> Classes { get; init; } = new List<string>();
		public IEnumerable<int> Years { get; init; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1).ToList();
		public IEnumerable<string> Tags { get; init; } = new List<string>();
		public IEnumerable<string> Genres { get; init; } = new List<string>();
		public IEnumerable<string> Flags { get; init; } = new List<string>();

		public bool ShowObsoleted { get; set; }
		public bool OnlyObsoleted { get; set; }
		public string SortBy { get; set; } = "";
		public int? Limit { get; set; }

		public IEnumerable<int> Authors { get; init; } = new List<int>();

		public IEnumerable<int> MovieIds { get; init; } = new List<int>();

		public IEnumerable<int> Games { get; init; } = new List<int>();
		public IEnumerable<int> GameGroups { get; init; } = new List<int>();
	}
}
