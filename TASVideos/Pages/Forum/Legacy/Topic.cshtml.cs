﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Legacy
{
	// Handles legacy forum links to viewTopic.php
	[AllowAnonymous]
	public class TopicModel : BaseForumModel
	{
		public TopicModel(ApplicationDbContext db, ITopicWatcher watcher)
			: base(db, watcher)
		{
		}

		[FromQuery]
		public int? P { get; set; }

		[FromQuery]
		public int? T { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			if (!P.HasValue && !T.HasValue && !Id.HasValue)
			{
				return NotFound();
			}

			if (P.HasValue)
			{
				var model = await GetPostPosition(P.Value, User.Has(PermissionTo.SeeRestrictedForums));
				if (model == null)
				{
					return NotFound();
				}

				return RedirectToPage(
					"/Forum/Topics/Index",
					new
					{
						Id = model.TopicId,
						Highlight = P,
						CurrentPage = model.Page
					});
			}

			return RedirectToPage("/Forum/Topics/Index", new { Id = T ?? Id });
		}
	}
}
