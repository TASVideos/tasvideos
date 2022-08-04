using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TASVideos.Data;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosData(this IServiceCollection services, IHostEnvironment env, string connectionString)
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		return services.AddDbContext<ApplicationDbContext>(
			options =>
			{
				options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TASVideos.Data").UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
					.UseSnakeCaseNamingConvention();

				if (env.IsDevelopment())
				{
					options.EnableSensitiveDataLogging(); // NEVER do this in production
				}
			});
	}
}
