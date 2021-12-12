using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.Data
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosData(this IServiceCollection services, string connectionString)
		{
			return services.AddDbContext<ApplicationDbContext>(
				options =>
					options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TASVideos.Data"))
						.UseSnakeCaseNamingConvention());
		}
	}
}
