﻿using Microsoft.Extensions.DependencyInjection;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Cache;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Core;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosCore<T>(this IServiceCollection services, bool isDevelopment, AppSettings settings) where T : class, IWikiToTextRenderer
	{
		services.AddScoped<IWikiToTextRenderer, T>();
		services
			.AddControllers()
			.AddApplicationPart(typeof(IJwtAuthenticator).Assembly);

		services
			.AddCacheService(settings.CacheSettings)
			.AddExternalMediaPublishing(isDevelopment, settings);

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
			.AddHttpClient(HttpClients.TwitterV2, client =>
			{
				client.BaseAddress = new Uri("https://api.twitter.com/2/tweets");
			});
		services
			.AddHttpClient(HttpClients.TwitterAuth, client =>
			{
				client.BaseAddress = new Uri("https://api.twitter.com/2/oauth2/token");
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

		return services.AddServices(settings);
	}

	private static IServiceCollection AddServices(this IServiceCollection services, AppSettings settings)
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
		services.AddScoped<IIpBanService, IpBanService>();
		services.AddScoped<ITagService, TagService>();
		services.AddScoped<IFlagService, FlagService>();
		services.AddScoped<IClassService, ClassService>();
		services.AddScoped<IMovieFormatDeprecator, MovieFormatDeprecator>();
		services.AddScoped<IForumService, ForumService>();

		services.AddScoped<IJwtAuthenticator, JwtAuthenticator>();

		if (settings.Gmail.IsEnabled())
		{
			services.AddTransient<IEmailSender, GmailSender>();
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
		if (cacheSettings.CacheType == "Memory")
		{
			services.AddMemoryCache();
			services.AddSingleton<ICacheService, MemoryCacheService>();
		}
		else if (cacheSettings.CacheType == "Redis")
		{
			services.AddScoped<ICacheService, RedisCacheService>();
		}
		else
		{
			services.AddSingleton<ICacheService, NoCacheService>();
		}

		services.AddScoped<RedisCacheService>(); // For Twitter tokens, we specifically need redis
		return services;
	}

	private static IServiceCollection AddExternalMediaPublishing(this IServiceCollection services, bool isDevelopment, AppSettings settings)
	{
		if (isDevelopment)
		{
			services.AddSingleton<IPostDistributor, LogDistributor>();
		}

		services.AddSingleton<IPostDistributor, IrcDistributor>();
		services.AddScoped<IPostDistributor, DiscordDistributor>();

		if (settings.TwitterV2.IsEnabled())
		{
			services.AddScoped<IPostDistributor, TwitterDistributorV2>();
		}

		services.AddScoped<IPostDistributor, DistributorStorage>();

		services.AddScoped<TwitterDistributorV2>();
		return services.AddTransient<ExternalMediaPublisher>();
	}
}
