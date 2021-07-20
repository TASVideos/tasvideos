using System;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.Core.Services.RssFeedParsers;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosCore(this IServiceCollection services, bool isDevelopment)
		{
			services
				.AddControllers()
				.AddApplicationPart(typeof(IJwtAuthenticator).Assembly);

			// HTTP Client
			services
				.AddHttpClient(HttpClients.Discord, client =>
				{
					client.BaseAddress = new Uri("https://discord.com/api/v6/");
				});
			services
				.AddHttpClient(HttpClients.Twitter, client =>
				{
					client.BaseAddress = new Uri("https://api.twitter.com/1.1/");
				});
			services
				.AddHttpClient(HttpClients.GoogleAuth, client =>
				{
					client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
				});
			services
				.AddHttpClient(HttpClients.Youtube, client =>
				{
					client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
				});

			return services.AddServices(isDevelopment);
		}

		private static IServiceCollection AddServices(this IServiceCollection services, bool isDevelopment)
		{
			services.AddScoped<UserManager>();
			services.AddScoped<SignInManager>();
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
			services.AddScoped<ITierService, TierService>();

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
			
			services.AddScoped<IYoutubeSync, YouTubeSync>();
			services.AddScoped<IGoogleAuthService, GoogleAuthService>();
			services.AddScoped<IPublicationMaintenanceLogger, PublicationMaintenanceLogger>();

			return services;
		}
	}
}
