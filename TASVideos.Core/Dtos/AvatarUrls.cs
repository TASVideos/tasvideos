using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services;

public record AvatarUrls(string? Avatar, string? MoodBase)
{
	public bool HasMoods => !string.IsNullOrWhiteSpace(MoodBase);
	public bool HasAvatar => !HasMoods && !string.IsNullOrWhiteSpace(Avatar);

	public string ToMoodUrl(ForumPostMood mood)
	{
		return MoodBase == null
			? ""
			: MoodBase.Replace("$", ((int)mood).ToString());
	}
}