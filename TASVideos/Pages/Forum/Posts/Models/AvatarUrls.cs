namespace TASVideos.Pages.Forum.Posts.Models;

public record AvatarUrls(string? Avatar, string? MoodBase)
{
	public bool HasMoods => !string.IsNullOrWhiteSpace(MoodBase);
	public bool HasAvatar => !HasMoods && !string.IsNullOrWhiteSpace(Avatar);
}
