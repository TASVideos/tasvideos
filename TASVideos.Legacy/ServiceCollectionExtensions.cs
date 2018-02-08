using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Extensions
{
	public static class ServiceCollectionExtensions
	{
		private const string MySqlSiteConnection = "server=localhost;userid=root;pwd=;port=3306;database=nesvideos_site;sslmode=none;";
		private const string MySqlForumConnection = "server=localhost;userid=root;pwd=;port=3306;database=nesvideos_forum;sslmode=none;";

		public static IServiceCollection AddLegacyContext(this IServiceCollection services)
		{
			services.AddDbContext<NesVideosSiteContext>(options =>
				options.UseMySQL(MySqlSiteConnection));

			services.AddDbContext<NesVideosForumContext>(options =>
				options.UseMySQL(MySqlForumConnection));

			return services;
		}
	}
}
