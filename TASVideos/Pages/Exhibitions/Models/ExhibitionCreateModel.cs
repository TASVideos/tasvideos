using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Models;

public class ExhibitionCreateModel
{
	public string Title { get; set; } = "";
	public string ExhibitionTimestamp { get; set; } = "";
	public List<int> Games { get; set; } = new();
	public List<int> Contributors { get; set; } = new();

	public List<ExhibitionFileCreateModel> Files { get; set; } = new();

	public List<ExhibitionUrlCreateModel> Urls { get; set; } = new();

	public class ExhibitionFileCreateModel
	{
		public string Path { get; set; } = "";
		public FileType Type { get; set; }
		public string Description { get; set; } = "";
	}

	public class ExhibitionUrlCreateModel
	{
		public string Url { get; set; } = "";
		public ExhibitionUrlType Type { get; set; }
		public string DisplayName { get; set; } = "";
	}
}
