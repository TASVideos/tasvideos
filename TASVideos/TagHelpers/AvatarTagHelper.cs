using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data.Entity.Forum;

namespace TASVideos.TagHelpers
{
	public class AvatarTagHelper : TagHelper
	{
		public ForumPostMood Mood { get; set; }
		public string Avatar { get; set; }
		public string MoodAvatarBase { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Mood == ForumPostMood.None)
			{
				output.SuppressOutput();
				return;
			}

			string avatarUrl = Avatar;

			if (!string.IsNullOrWhiteSpace(MoodAvatarBase))
			{
				avatarUrl = MoodAvatarBase.Replace("$", ((int)Mood).ToString());
			}

			output.TagName = "img";
			output.Attributes.Clear();
			output.Attributes.Add("src", avatarUrl);
		}
	}
}
