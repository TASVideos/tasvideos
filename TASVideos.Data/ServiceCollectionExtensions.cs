using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.Data
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosData(this IServiceCollection services, IConfiguration configuration, bool usePostgres)
		{
			services.AddDbContext(configuration, usePostgres);
			return services;
		}

		internal static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration, bool usePostgres)
		{
			if (usePostgres)
			{
				services.AddDbContext<ApplicationDbContext>(
					options =>
						options.UseNpgsql(
							configuration.GetConnectionString("PostgresConnection"),
							b => b.MigrationsAssembly("TASVideos.Data"))
							.UseSnakeCaseNamingConvention());
			}
			else
			{
				services.AddDbContext<ApplicationDbContext>(
					options =>
						options.UseSqlServer(
							configuration.GetConnectionString("DefaultConnection"),
							b => b.MigrationsAssembly("TASVideos.Data")));
			}

			return services;
		}
	}
}
