using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Forum;
using TASVideos.Extensions;

namespace TASVideos.Pages.Forum
{
	public class BaseForumModel : BasePageModel
	{
		protected static readonly IEnumerable<SelectListItem> TopicTypeList = Enum
			.GetValues(typeof(ForumTopicType))
			.Cast<ForumTopicType>()
			.Select(m => new SelectListItem
			{
				Value = ((int)m).ToString(),
				Text = m.EnumDisplayName()
			})
			.ToList();

		protected static readonly IEnumerable<SelectListItem> MoodList = Enum
			.GetValues(typeof(ForumPostMood))
			.Cast<ForumPostMood>()
			.Select(m => new SelectListItem
			{
				Value = ((int)m).ToString(),
				Text = m.EnumDisplayName(),
				Group = m >= ForumPostMood.AltNormal ? AltGroup : StandardGroup
			})
			.ToList();

		private static readonly SelectListGroup StandardGroup = new () { Name = "Standard" };
		private static readonly SelectListGroup AltGroup = new () { Name = "Alternate" };

		public IEnumerable<SelectListItem> Moods => MoodList;
		public IEnumerable<SelectListItem> TopicTypes => TopicTypeList;

		public new IActionResult NotFound()
		{
			return RedirectToPage("/Forum/NotFound");
		}
	}
}
