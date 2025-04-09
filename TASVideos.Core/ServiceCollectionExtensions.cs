using Microsoft.Extensions.DependencyInjection;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Cache;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Core.Services.Forum;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Core;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosCore<T1, T2>(this IServiceCollection services, bool isDevelopment, AppSettings settings)
		where T1 : class, IWikiToTextRenderer
		where T2 : class, IWikiToMetaDescriptionRenderer
	{
		services.AddScoped<IWikiToTextRenderer, T1>();
		services.AddScoped<IWikiToMetaDescriptionRenderer, T2>();
		services
			.AddCacheService(settings.CacheSettings)
			.AddExternalMediaPublishing(settings, isDevelopment);

		// HTTP Client
		services
			.AddHttpClient(HttpClients.Discord, client =>
			{
				client.BaseAddress = new Uri("https://discord.com/api/v10/");
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
		services
			.AddHttpClient(HttpClients.Bluesky, client =>
			{
				client.BaseAddress = new Uri("https://bsky.social/xrpc/");
			});

		return services.AddServices(settings);
	}

	private static IServiceCollection AddServices(this IServiceCollection services, AppSettings settings)
	{
		services.AddScoped<UserManager>();
		services.AddScoped<IUserManager, UserManager>();
		services.AddScoped<SignInManager>();
		services.AddScoped<ISignInManager, SignInManager>();
		services.AddScoped<IPointsService, PointsService>();
		services.AddScoped<IAwards, Awards>();
		services.AddScoped<IMediaFileUploader, MediaFileUploader>();
		services.AddScoped<ILanguages, Languages>();
		services.AddScoped<ITopicWatcher, TopicWatcher>();
		services.AddScoped<IPublicationHistory, PublicationHistory>();
		services.AddScoped<IFileService, FileService>();
		services.AddScoped<IMovieSearchTokens, MovieSearchTokens>();
		services.AddScoped<IIpBanService, IpBanService>();
		services.AddScoped<ITagService, TagService>();
		services.AddScoped<IFlagService, FlagService>();
		services.AddScoped<IGenreService, GenreService>();
		services.AddScoped<IClassService, ClassService>();
		services.AddScoped<IMovieFormatDeprecator, MovieFormatDeprecator>();
		services.AddScoped<IForumService, ForumService>();
		services.AddScoped<IGameSystemService, GameSystemService>();
		services.AddScoped<IPrivateMessageService, PrivateMessageService>();
		services.AddScoped<IRoleService, RoleService>();
		services.AddScoped<IRatingService, RatingService>();

		services.AddScoped<IJwtAuthenticator, JwtAuthenticator>();

		if (settings.Email.IsEnabled())
		{
			services.AddTransient<IEmailSender, SmtpSender>();
		}
		else
		{
			services.AddTransient<IEmailSender, EmailLogger>();
		}

		services.AddTransient<IEmailService, EmailService>();
		services.AddTransient<IWikiPages, WikiPages>();

		services.AddScoped<ITASVideoAgent, TASVideoAgent>();
		services.AddScoped<ITASVideosGrue, TASVideosGrue>();

		services.AddScoped<ForumEngine.IWriterHelper, ForumWriterHelper>();
		services.AddScoped<IForumToMetaDescriptionRenderer, ForumToMetaDescriptionRenderer>();

		services.AddScoped<IYoutubeSync, YouTubeSync>();
		services.AddScoped<IGoogleAuthService, GoogleAuthService>();
		services.AddScoped<IPublicationMaintenanceLogger, PublicationMaintenanceLogger>();
		services.AddScoped<IUserMaintenanceLogger, UserMaintenanceLogger>();
		services.AddScoped<IQueueService, QueueService>();
		services.AddScoped<IUserFiles, UserFiles>();

		return services;
	}

	private static IServiceCollection AddCacheService(this IServiceCollection services, AppSettings.CacheSetting cacheSettings)
	{
		switch (cacheSettings.CacheType)
		{
			case "Memory":
				services.AddMemoryCache();
				services.AddSingleton<ICacheService, MemoryCacheService>();
				break;
			case "Redis":
				services.AddScoped<ICacheService, RedisCacheService>();
				break;
			default:
				services.AddSingleton<ICacheService, NoCacheService>();
				break;
		}

		return services;
	}

	private static IServiceCollection AddExternalMediaPublishing(this IServiceCollection services, AppSettings settings, bool isDevelopment)
	{
		if (isDevelopment)
		{
			services.AddSingleton<IPostDistributor, LogDistributor>();
		}

		if (settings.Irc.IsEnabled())
		{
			services.AddSingleton<IPostDistributor, IrcDistributor>();
		}

		if (settings.Discord.IsEnabled())
		{
			services.AddScoped<IPostDistributor, DiscordDistributor>();
		}

		if (settings.Bluesky.IsEnabled())
		{
			services.AddScoped<IPostDistributor, BlueskyDistributor>();
		}

		return services.AddScoped<IPostDistributor, DistributorStorage>();
	}
}
