using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TASVideos.Api.Validators;
using TASVideos.Core.Settings;

namespace TASVideos.Api;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosApi(this IServiceCollection services, AppSettings settings)
	{
		return services
			.AddProblemDetails()
			.AddScoped<IValidator<ApiRequest>, ApiRequestValidator>()
			.AddScoped<IValidator<AuthenticationRequest>, AuthenticationRequestValidator>()
			.AddScoped<IValidator<GamesRequest>, GamesRequestValidator>()
			.AddScoped<IValidator<PublicationsRequest>, PublicationsRequestValidator>()
			.AddScoped<IValidator<SubmissionsRequest>, SubmissionsRequestValidator>()
			.AddScoped<IValidator<TagAddEditRequest>, TagAddEditRequestValidator>()
			.AddSwagger(settings);
	}

	private static IServiceCollection AddSwagger(this IServiceCollection services, AppSettings settings)
	{
		services.AddAuthentication(x =>
		{
			x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(x =>
		{
			x.RequireHttpsMetadata = true;
			x.SaveToken = true;
			x.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.Jwt.SecretKey)),
				ValidateIssuer = false,
				ValidateAudience = false
			};
		});

		var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

		services.AddEndpointsApiExplorer();
		return services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc(
				"v1",
				new OpenApiInfo
				{
					Title = "TASVideos API",
					Version = $"v{version.Major}.{version.Minor}.{version.Revision}",
					Description = "API For tasvideos.org content"
				});
			c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
			{
				Name = "Authorization"
			});
		});
	}
}
