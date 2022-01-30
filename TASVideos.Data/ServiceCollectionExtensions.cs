using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.Data;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosData(this IServiceCollection services, string connectionString)
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		return services.AddDbContext<ApplicationDbContext>(
			options =>
				options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TASVideos.Data"))
					.UseSnakeCaseNamingConvention());
	}
}
