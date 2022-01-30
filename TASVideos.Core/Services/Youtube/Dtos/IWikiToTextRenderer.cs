using TASVideos.Data.Entity;

namespace TASVideos.Core.Services.Youtube;

public interface IWikiToTextRenderer
{
	Task<string> RenderWikiForYoutube(WikiPage page);
}
