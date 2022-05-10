using TASVideos.Models;

namespace TASVideos.Pages.UserFiles.Models;

public class GameFileModel
{
	public int GameId { get; set; }
	public string GameName { get; set; } = "";

	public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();
}
