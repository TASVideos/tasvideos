using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Models
{
	public class PublicationSearchModel
	{
		public IEnumerable<string> SystemCodes { get; set; } = new List<string>();
		public IEnumerable<string> Tiers { get; set; } = new List<string>();
		public IEnumerable<int> Years { get; set; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1);
		public bool ShowObsoleted { get; set; }
	}

	public class PublicationViewModel
	{
		public int Id { get; set; }
		public DateTime CreateTimeStamp { get; set; }
		public int? ObsoletedBy { get; set; }
		public string Title { get; set; }
		public string Screenshot { get; set; }
		public string TorrentLink { get; set; }
		public string MovieFileName { get; set; }
		public int SubmissionId { get; set; }
		public string OnlineWatchingUrl { get; set; }
		public string MirrorSiteUrl { get; set; }
	}
}
