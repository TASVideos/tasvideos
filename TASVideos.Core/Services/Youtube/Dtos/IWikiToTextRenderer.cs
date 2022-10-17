using TASVideos.Core.Services.Wiki;

namespace TASVideos.Core.Services.Youtube;

public interface IWikiToTextRenderer
{
	Task<string> RenderWikiForYoutube(IWikiPage page);
}
