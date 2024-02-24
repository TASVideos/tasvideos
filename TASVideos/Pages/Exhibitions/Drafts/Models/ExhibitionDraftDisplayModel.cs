using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Drafts.Models;

public class ExhibitionDraftDisplayModel
{
	public int Id { get; set; }
	public string Title { get; set; } = "";
	public int? TopicId { get; set; }
	public DateTime ExhibitionTimestamp { get; set; }
	public DateTime CreateTimestamp { get; set; }
	public List<GameModel> Games { get; set; } = [];
	public List<UserModel> Contributors { get; set; } = [];
	public List<UrlModel> Urls { get; set; } = [];
	public List<FileModel> Files { get; set; } = [];

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

	public class FileModel
	{
		public ExhibitionFileType Type { get; set; }
		public string? Description { get; set; }
		public string Path { get; set; } = "";
	}
}
