namespace TASVideos.ViewComponents.Models
{
	public class GameNameModel
	{
		public int GameId { get; set; }
		public string DisplayName { get; set; }

		public string System { get; set; }

		public bool IsSystem => !string.IsNullOrWhiteSpace(System);
	}
}
