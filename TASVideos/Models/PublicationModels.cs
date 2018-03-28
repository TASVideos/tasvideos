using System;
using System.Collections.Generic;
using System.Linq;

using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	public class PublicationSearchModel
	{
		public IEnumerable<string> SystemCodes { get; set; } = new List<string>();
		public IEnumerable<string> Tiers { get; set; } = new List<string>();
		public IEnumerable<int> Years { get; set; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1);
		public IEnumerable<string> Tags { get; set; } = new List<string>();
		public bool ShowObsoleted { get; set; }
	}

	public class PublicationViewModel
	{
		public int Id { get; set; }
		public DateTime CreateTimeStamp { get; set; }
		public int? ObsoletedBy { get; set; }
		public string Title { get; set; }
		
		public string MovieFileName { get; set; }
		public int SubmissionId { get; set; }
		public string OnlineWatchingUrl { get; set; }
		public string MirrorSiteUrl { get; set; }

		public IEnumerable<TagModel> Tags { get; set; } = new List<TagModel>();
		public IEnumerable<FileModel> Files { get; set; } = new List<FileModel>();

		public string Screenshot => Files.First(f => f.Type == FileType.Screenshot).Path;
		public string TorrentLink => Files.First(f => f.Type == FileType.Torrent).Path;

		public class TagModel
		{
			public string DisplayName { get; set; }
			public string Code { get; set; }
		}

		public class FileModel
		{
			public string Path { get; set; }
			public FileType Type { get; set; }
		}
	}

	public class TabularMovieListSearchModel
	{
		public int Limit { get; set; } = 10;
		public IEnumerable<string> Tiers { get; set; } = new List<string>();
	}

	public class TabularMovieListResultModel
	{
		public int Id { get; set; }
		public DateTime CreateTimeStamp { get; set; }
		public TimeSpan Time { get; set; }

		public int? ObsoletedBy { get; set; }
		public TimeSpan? PreviousTime { get; set; }
		public int PreviousId { get; set; }

		public string Game { get; set; }
		public string Authors { get; set; }
	}
}
