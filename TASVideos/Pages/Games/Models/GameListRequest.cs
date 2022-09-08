using TASVideos.Core;

namespace TASVideos.Pages.Games.Models;

public class GameListRequest : PagingModel
{
	public GameListRequest()
	{
		PageSize = 50;
		Sort = "DisplayName";
	}

	public string? SystemCode { get; set; }

	public string? Letter { get; set; }

	public string? SearchTerms { get; set; }
}
