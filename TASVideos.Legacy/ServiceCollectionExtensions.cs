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
			string mySqlSiteConnection,
			string mySqlForumConnection,
			bool enable)
		{
			if (enable)
			{
				services.AddDbContext<NesVideosSiteContext>(options =>
					options.UseMySql(mySqlSiteConnection ?? DefaultMySqlSiteConnection));

				services.AddDbContext<NesVideosForumContext>(options =>
					options.UseMySql(mySqlForumConnection ?? DefaultMySqlForumConnection));
			}

			return services;
		}
	}
}
