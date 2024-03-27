namespace TASVideos.Pages.Forum.Posts.Models;

public record MiniPostModel(
	DateTime CreateTimestamp,
	string PosterName,
	PreferredPronounTypes PosterPronouns,
	string Text,
	bool EnableHtml,
	bool EnableBbCode);
