using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionDisplayModel
	{
		public int TopicId { get; set; }
		public bool CanEdit { get; set; }
		public bool IsCataloged => SystemId.HasValue
			&& SystemFrameRateId.HasValue
			&& GameId.HasValue
			&& RomId.HasValue;

		[Display(Name = "For tier")]
		public string TierName { get; set; }

		[Display(Name = "Console")]
		public string SystemDisplayName { get; set; }

		public string SystemCode { get; set; }

		[Display(Name = "Game name")]
		public string GameName { get; set; }

		[Display(Name = "Game Version")]
		public string GameVersion { get; set; }

		[Display(Name = "ROM filename")]
		public string RomName { get; set; }

		[Display(Name = "Branch")]
		public string Branch { get; set; }

		[Display(Name = "Emulator")]
		public string Emulator { get; set; }

		[Url]
		[Display(Name = "Encode Embed Link")]
		public string EncodeEmbedLink { get; set; }

		[Display(Name = "FrameCount")]
		public int FrameCount { get; set; }

		public double FrameRate { get; set; }

		public int RerecordCount { get; set; }

		[Display(Name = "Author")]
		public IEnumerable<string> Authors { get; set; } = new List<string>();

		[Display(Name = "Submitter")]
		public string Submitter { get; set; }

		[Display(Name = "Submit Date")]
		public DateTime CreateTimestamp { get; set; }

		[Display(Name = "Last Edited")]
		public DateTime LastUpdateTimeStamp { get; set; }

		[Display(Name = "Last Edited by")]
		public string LastUpdateUser { get; set; }

		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		[Display(Name = "Judge")]
		public string Judge { get; set; }

		[Display(Name = "Publisher")]
		public string Publisher { get; set; }

		public string Title { get; set; }

		internal int? SystemId { get; set; }
		internal int? SystemFrameRateId { get; set; }
		internal int? GameId { get; set; }
		internal int? RomId { get; set; }
	}
}
