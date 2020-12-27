using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Extensions
{
	public static class ServiceCollectionExtensions
	{
		private const string DefaultMySqlSiteConnection = "server=localhost;userid=root;pwd=;port=3306;database=nesvideos_site;sslmode=none;";
		private const string DefaultMySqlForumConnection = "server=localhost;userid=root;pwd=;port=3306;database=nesvideos_forum;sslmode=none;";

		public static IServiceCollection AddTasVideosLegacy(
			this IServiceCollection services,
			string? mySqlSiteConnection,
			string? mySqlForumConnection,
			bool enable)
		{
			if (enable)
			{
				string siteConnectionString = mySqlSiteConnection ?? DefaultMySqlSiteConnection;
				string forumConnectionString = mySqlForumConnection ?? DefaultMySqlForumConnection;

				services.AddDbContext<NesVideosSiteContext>(options =>
					options.UseMySql(siteConnectionString, ServerVersion.AutoDetect(siteConnectionString)));

				services.AddDbContext<NesVideosForumContext>(options =>
					options.UseMySql(forumConnectionString, ServerVersion.AutoDetect(forumConnectionString)));
			}

			return services;
		}
	}
}
