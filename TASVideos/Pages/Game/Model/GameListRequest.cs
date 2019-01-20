using TASVideos.Data;

namespace TASVideos.Pages.Game.Model
{
	public class GameListRequest : PagedModel
	{
		public GameListRequest()
		{
			PageSize = 25;
		}

		public string SystemCode { get; set; }
	}
}
