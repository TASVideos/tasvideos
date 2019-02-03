using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Game.Model
{
	public class GameEditModel
	{
		[Required]
		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Required]
		[Display(Name = "Good Name")]
		public string GoodName { get; set; }

		[Required]
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		[Required]
		[Display(Name = "Abbreviation")]
		public string Abbreviation { get; set; }

		[Required]
		[Display(Name = "Search Key")]
		public string SearchKey { get; set; }

		[Required]
		[Display(Name = "Youtube Tags")]
		public string YoutubeTags { get; set; }

		[Display(Name = "Screenshot Url")]
		public string ScreenshotUrl { get; set; }
	}
}
