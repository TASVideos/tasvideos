using TASVideos.Core;

namespace TASVideos.RazorPages.Pages.Games.Models
{
	public class GameListRequest : PagingModel
	{
		public GameListRequest()
		{
			PageSize = 25;
			Sort = "DisplayName";
		}

		public string? SystemCode { get; set; }
	}
}
