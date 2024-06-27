namespace TASVideos.Core.Services.Wiki;

public interface IWikiToMetaDescriptionRenderer
{
	Task<string> RenderWikiForMetaDescription(IWikiPage page);
}
