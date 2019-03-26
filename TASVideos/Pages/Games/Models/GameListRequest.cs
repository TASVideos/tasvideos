using TASVideos.Data;

namespace TASVideos.Pages.Games.Models
{
	public class GameListRequest : PagingModel
	{
		public GameListRequest()
		{
			PageSize = 25;
		}

		public string SystemCode { get; set; }
	}
}
