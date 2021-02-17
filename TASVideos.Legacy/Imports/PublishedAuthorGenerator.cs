using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;

namespace TASVideos.Legacy.Imports
{
	public static class PublishedAuthorGenerator
	{
		public static void Generate(string connectionStr, ApplicationDbContext context)
		{
			var publishedAuthorRole = context.Roles.Single(r => r.Name == RoleSeedNames.PublishedAuthor);

			var userRoles = context.Users
				.Where(u => u.Publications.Any())
				.Select(u => new UserRole
				{
					UserId = u.Id,
					RoleId = publishedAuthorRole.Id
				})
				.ToList();

			var columns = new[]
			{
				nameof(UserRole.UserId),
				nameof(UserRole.RoleId)
			};

			userRoles.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.UserRoles));
		}
	}
}
