using System;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.Core.Services.RssFeedParsers;
using TASVideos.Core.Settings;

namespace TASVideos.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosCore(this IServiceCollection services, AppSettings settings, bool isDevelopment)
		{
			services
				.AddControllers()
				.AddApplicationPart(typeof(IJwtAuthenticator).Assembly);

			// HTTP Client
			services
				.AddHttpClient("Discord", client =>
				{
					client.BaseAddress = new Uri("https://discord.com/api/v6/");
				});
			services
				.AddHttpClient("Twitter", client =>
				{
					client.BaseAddress = new Uri("https://api.twitter.com/1.1/");
				});

			services.AddServices(settings, isDevelopment);
			return services;
		}

		private static IServiceCollection AddServices(this IServiceCollection services, AppSettings settings, bool isDevelopment)
		{
			services.AddScoped<UserManager>();
			services.AddScoped<IPointsService, PointsService>();
			services.AddScoped<IAwards, Awards>();
			services.AddScoped<IMediaFileUploader, MediaFileUploader>();
			services.AddScoped<ILanguages, Languages>();
			services.AddScoped<ITopicWatcher, TopicWatcher>();
			services.AddScoped<IPublicationHistory, PublicationHistory>();
			services.AddScoped<IFileService, FileService>();
			services.AddScoped<IMovieSearchTokens, MovieSearchTokens>();
			services.AddScoped<IVcsRssParser, VcsRssParser>();
			services.AddScoped<IIpBanService, IpBanService>();
			services.AddScoped<ITagService, TagService>();
			services.AddScoped<IFlagService, FlagService>();

			services.AddSingleton(settings.Jwt); // TODO: just register settings in mvc project
			services.AddScoped<IJwtAuthenticator, JwtAuthenticator>();

			if (isDevelopment)
			{
				services.AddTransient<IEmailSender, EmailLogger>();
			}
			else
			{
				services.AddTransient<IEmailSender, SendGridSender>();
			}

			services.AddTransient<IEmailService, EmailService>();
			services.AddTransient<IWikiPages, WikiPages>();

			services.AddScoped<ITASVideoAgent, TASVideoAgent>();
			services.AddScoped<ITASVideosGrue, TASVideosGrue>();

			services.AddScoped<ForumEngine.IWriterHelper, ForumWriterHelper>();

			return services;
		}

	}
}
