using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Models;

public class ExhibitionEditModel
{
	public string Title { get; set; } = "";
	public string ExhibitionTimestamp { get; set; } = "";
	public List<int> Games { get; set; } = new();
	public List<int> Contributors { get; set; } = new();

	public List<ExhibitionFileDisplayModel> Files { get; set; } = new();

	public List<ExhibitionUrlDisplayModel> Urls { get; set; } = new();

	public class ExhibitionFileDisplayModel
	{
		public string Path { get; set; } = "";
		public ExhibitionFileType Type { get; set; }
		public string Description { get; set; } = "";
	}

	public class ExhibitionUrlDisplayModel
	{
		public string Url { get; set; } = "";
		public ExhibitionUrlType Type { get; set; }
		public string DisplayName { get; set; } = "";
	}
}
