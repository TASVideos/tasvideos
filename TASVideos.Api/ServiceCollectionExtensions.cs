using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.Api;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosApi(this IServiceCollection services)
	{
		return services.AddValidatorsFromAssemblyContaining<ApiRequest>();
	}
}
