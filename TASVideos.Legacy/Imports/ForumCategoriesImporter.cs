using System;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
    public static class ForumCategoriesImporter
    {
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var categories = legacyForumContext
				.Categories
				.Select(c => new ForumCategory
				{
					Id = c.Id,
					Title = c.Title,
					Ordinal =  c.Title == "Other"
						? 30
						: c.Title == "Completed movies"
							? 40
							: c.Title == "Emulators"
								? 50
								: c.Order,
					Description = c.Description,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow,
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
				nameof(ForumCategory.CreateTimeStamp),
				nameof(ForumCategory.LastUpdateTimeStamp),
				nameof(ForumCategory.CreateUserName),
				nameof(ForumCategory.LastUpdateUserName)
			};

			categories.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.ForumCategories));
		}
    }
}
