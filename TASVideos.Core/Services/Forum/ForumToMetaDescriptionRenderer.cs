using System.Text;
using TASVideos.ForumEngine;

namespace TASVideos.Core.Services.Forum;

public interface IForumToMetaDescriptionRenderer
{
	Task<string> RenderForumForMetaDescription(string text, bool enableBbCode, bool enableHtml);
}

public class ForumToMetaDescriptionRenderer(IWriterHelper helper) : IForumToMetaDescriptionRenderer
{
	public async Task<string> RenderForumForMetaDescription(string text, bool enableBbCode, bool enableHtml)
	{
		Element content;
		if (enableHtml || enableBbCode)
		{
			content = BbParser.Parse(text, enableHtml, enableBbCode);
		}
		else
		{
			content = new Element { Name = "_root" };
			content.Children.Add(new Text { Content = text });
		}

		var sb = new StringBuilder();
		await content.WriteMetaDescription(sb, helper);
		return sb.ToString().Trim();
	}
}
