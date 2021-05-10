using System;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	internal static class ForumImporter
	{
		public static void Import(NesVideosForumContext legacyForumContext)
		{
			var forums = legacyForumContext
				.Forums
				.Select(f => new Forum
				{
					Id = f.Id,
					CategoryId = f.CategoryId,
					Name = ImportHelper.ConvertNotNullLatin1String(f.Name),
					Description = ImportHelper.ConvertLatin1String(f.Description),
					Ordinal = f.Order,
					ShortName = f.ShortName,
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow,
					CreateUserName = "LegacyImport",
					LastUpdateUserName = "LegacyImport",
					Restricted = f.AuthView == 2
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
				nameof(Forum.CreateTimestamp),
				nameof(Forum.LastUpdateTimestamp),
				nameof(Forum.CreateUserName),
				nameof(Forum.LastUpdateUserName),
				nameof(Forum.Restricted)
			};

			forums.BulkInsert(columns, nameof(ApplicationDbContext.Forums));
		}
	}
}
