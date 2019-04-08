using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Games.Models
{
	public class GameEditModel
	{
		[Required]
		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Required]
		[Display(Name = "Good Name")]
		[StringLength(250)]
		public string GoodName { get; set; }

		[Required]
		[StringLength(100)]
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		[Required]
		[StringLength(8)]
		[Display(Name = "Abbreviation")]
		public string Abbreviation { get; set; }

		[Required]
		[StringLength(64)]
		[Display(Name = "Search Key")]
		public string SearchKey { get; set; }

		[Required]
		[StringLength(250)]
		[Display(Name = "Youtube Tags")]
		public string YoutubeTags { get; set; }

		[StringLength(250)]
		[Display(Name = "Screenshot Url")]
		public string ScreenshotUrl { get; set; }

		[Display(Name = "Genres")]
		public IEnumerable<int> Genres { get; set; } = new List<int>();
	}
}
