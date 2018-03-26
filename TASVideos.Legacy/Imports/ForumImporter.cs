using System;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
    public static class ForumImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var forums = legacyForumContext
				.Forums
				.Select(f => new Forum
				{
					Id = f.Id,
					CategoryId = f.CategoryId,
					Name = ImportHelper.FixString(f.Name),
					Description = ImportHelper.FixString(f.Description),
					Ordinal = f.Order,
					ShortName = f.ShortName,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow,
					CreateUserName = "LegacyImport",
					LastUpdateUserName = "LegacyImport"
				})
				.ToList();

			var columns = new[]
			{
				nameof(Forum.Id),
				nameof(Forum.CategoryId),
				nameof(Forum.Name),
				nameof(Forum.ShortName),
				nameof(Forum.Ordinal),
				nameof(Forum.Description),
				nameof(Forum.CreateTimeStamp),
				nameof(Forum.LastUpdateTimeStamp),
				nameof(Forum.CreateUserName),
				nameof(Forum.LastUpdateUserName)
			};

			forums.BulkInsert(context, columns, nameof(ApplicationDbContext.Forums));
		}
	}
}
