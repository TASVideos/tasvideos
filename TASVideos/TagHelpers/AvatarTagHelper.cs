using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data.Entity.Forum;

namespace TASVideos.TagHelpers;

public class AvatarTagHelper : TagHelper
{
	public ForumPostMood Mood { get; set; }
	public string Avatar { get; set; } = "";
	public string MoodAvatarBase { get; set; } = "";

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (Mood == ForumPostMood.None)
		{
			output.SuppressOutput();
			return;
		}

		output.TagName = "img";
		output.Attributes.Add("loading", "lazy");

		string avatarUrl = Avatar;

		if (!string.IsNullOrWhiteSpace(MoodAvatarBase))
		{
			avatarUrl = MoodAvatarBase.Replace("$", ((int)Mood).ToString());
			if (Mood != ForumPostMood.Normal && Mood != ForumPostMood.AltNormal)
			{
				output.Attributes.Add("title", $"Mood: {Mood}");
			}
		}

		if (string.IsNullOrWhiteSpace(avatarUrl))
		{
			output.SuppressOutput();
			return;
		}

		output.Attributes.Add("src", avatarUrl);
	}
}
