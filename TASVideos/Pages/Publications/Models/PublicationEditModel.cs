using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Models
{
	public class PublicationEditModel
	{
		public string SystemCode { get; set; }

		public string Title { get; set; }

		[Display(Name = "Tier")]
		public string Tier { get; set; }
		public string TierIconPath { get; set; }
		public string TierLink { get; set; }

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

		[Display(Name = "Selected Flags")]
		public IEnumerable<int> SelectedFlags { get; set; } = new List<int>();

		[Display(Name = "Selected Tags")]
		public IEnumerable<int> SelectedTags { get; set; } = new List<int>();

		[Display(Name = "Revision Message")]
		public string RevisionMessage { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		public string Markup { get; set; }
	}

	public class PublicationFileDisplayModel
	{
		public int Id { get; set; }
		public string Path { get; set; }
		public FileType Type { get; set; }
		public string Description { get; set; }
	}
}
