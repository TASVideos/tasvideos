namespace TASVideos.Pages.Messages.Models;

public class PrivateMessageModel
{
	public string? Subject { get; init; }
	public DateTime SentOn { get; init; }
	public string Text { get; init; } = "";
	public int FromUserId { get; init; }
	public string FromUserName { get; init; } = "";

	public int ToUserId { get; init; }
	public string ToUserName { get; init; } = "";
}
