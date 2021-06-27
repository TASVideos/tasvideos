﻿using System;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	internal static class ForumCategoriesImporter
	{
		public static void Import(NesVideosForumContext legacyForumContext)
		{
			var categories = legacyForumContext
				.Categories
				.Select(c => new ForumCategory
				{
					Id = c.Id,
					Title = c.Title ?? "",
					Ordinal = c.Title == "Other"
						? 30
						: c.Title == "Completed movies"
							? 40
							: c.Title == "Emulators"
								? 50
								: c.Order,
					Description = c.Description,
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow,
					CreateUserName = "LegacyImport",
					LastUpdateUserName = "LegacyImport"
				})
				.ToList();

			var columns = new[]
			{
				nameof(ForumCategory.Id),
				nameof(ForumCategory.Title),
				nameof(ForumCategory.Ordinal),
				nameof(ForumCategory.Description),
				nameof(ForumCategory.CreateTimestamp),
				nameof(ForumCategory.LastUpdateTimestamp),
				nameof(ForumCategory.CreateUserName),
				nameof(ForumCategory.LastUpdateUserName)
			};

			categories.BulkInsert(columns, nameof(ApplicationDbContext.ForumCategories));
		}
	}
}
