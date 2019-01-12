using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class RenderWikiPageModel
	{
		public string Markup { get; set; }

		public WikiPage PageData { get; set; }
	}
}
