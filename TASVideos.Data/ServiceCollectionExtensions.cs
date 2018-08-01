using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TASVideos.Data
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosData(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext(configuration);
			return services;
		}

		internal static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			return services;
		}
	}
}
