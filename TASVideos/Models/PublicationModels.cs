using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;

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

	public class PublicationEditModel
	{
		public int Id { get; set; }
		public string SystemCode { get; set; }

		public string Title { get; set; }

		[Display(Name = "Obsoleted By")]
		public int? ObsoletedBy { get; set; }

		[Url]
		[Display(Name = "Online-watching URL")]
		public string OnlineWatchingUrl { get; set; }

		[Url]
		[Display(Name = "Mirror site URL")]
		public string MirrorSiteUrl { get; set; }

		[StringLength(50)]
		[Display(Name = "Emulator Version")]
		public string EmulatorVersion { get; set; }

		public string Branch { get; set; }

		public IEnumerable<SelectListItem> AvailableMoviesForObsoletedBy { get; set; } = new List<SelectListItem>();
	}

	public class PublicationCatalogModel
	{
		public int Id { get; set; }

		[Display(Name = "Rom")]
		public int RomId { get; set; }

		[Display(Name = "Game")]
		public int GameId { get; set; }

		[Display(Name = "System")]
		public int SystemId { get; set; }

		[Display(Name = "System Framerate")]
		public int SystemFrameRateId { get; set; }

		public IEnumerable<SelectListItem> AvailableRoms { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystemFrameRates { get; set; } = new List<SelectListItem>();
	}
}
