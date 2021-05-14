using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.Data
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosData(this IServiceCollection services, IConfiguration configuration, bool usePostgres)
		{
			return usePostgres
				? services.AddPostgresServerDbContext(configuration.GetConnectionString("PostgresConnection"))
				: services.AddSqlServerDbContext(configuration.GetConnectionString("DefaultConnection"));
		}

		internal static IServiceCollection AddPostgresServerDbContext(this IServiceCollection services, string postgresConnectionStr)
		{
			return services.AddDbContext<ApplicationDbContext>(
				options =>
					options.UseNpgsql(postgresConnectionStr, b => b.MigrationsAssembly("TASVideos.Data"))
						.UseSnakeCaseNamingConvention());
		}

		internal static IServiceCollection AddSqlServerDbContext(this IServiceCollection services, string sqlServerConnectionStr)
		{
			return services.AddDbContext<ApplicationDbContext>(
				options =>
					options.UseSqlServer(sqlServerConnectionStr, b => b.MigrationsAssembly("TASVideos.Data")));
		}
	}
}
