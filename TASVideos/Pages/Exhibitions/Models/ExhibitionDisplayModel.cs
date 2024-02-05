using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Models;

public class ExhibitionDisplayModel
{
	public int Id { get; set; }
	public IReadOnlyCollection<GameModel> Games { get; set; } = new List<GameModel>();
	public IReadOnlyCollection<UserModel> Contributors { get; set; } = new List<UserModel>();
	public IReadOnlyCollection<UrlModel> Urls { get; set; } = new List<UrlModel>();
	//public virtual ICollection<ExhibitionFile> Files { get; set; } = new HashSet<ExhibitionFile>();

	public string Title { get; set; } = "";

	public DateTime ExhibitionTimestamp { get; set; }

	public class GameModel
	{
		public int Id { get; set; }
		public string DisplayName { get; set; } = "";
	}
	public class UserModel
	{
		public int Id { get; set; }
		public string UserName { get; set; } = "";
	}
	public class UrlModel
	{
		public string Url { get; set; } = "";
		public ExhibitionUrlType Type;
		public string? DisplayName { get; set; }
	}
}
