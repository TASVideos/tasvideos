namespace TASVideos.Core.Services;

public class PrivateMessageDto
{
	public string? Subject { get; init; }
	public DateTime SentOn { get; init; }
	public string Text { get; init; } = "";
	public int FromUserId { get; init; }
	public string FromUserName { get; init; } = "";

	public int ToUserId { get; init; }
	public string ToUserName { get; init; } = "";

	public bool CanReply { get; init; }

	public bool EnableBbCode { get; init; }
	public bool EnableHtml { get; init; }
}
