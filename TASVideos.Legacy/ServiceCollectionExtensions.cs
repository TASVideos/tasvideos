using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Legacy.Data;

namespace TASVideos.Legacy.Extensions
{
	public static class ServiceCollectionExtensions
	{
		private const string MySqlConnection = "server=localhost;userid=root;pwd=Password1234!@#$;port=3306;database=nesvideos_site;sslmode=none;";

		public static IServiceCollection AddLegacyContext(this IServiceCollection services)
		{
			services.AddDbContext<NesVideosSiteContext>(options =>
				options.UseMySQL(MySqlConnection));

			return services;
		}
	}
}
