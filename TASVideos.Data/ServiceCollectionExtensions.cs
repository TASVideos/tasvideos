using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
