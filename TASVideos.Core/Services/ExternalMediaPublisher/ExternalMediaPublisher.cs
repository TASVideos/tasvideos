using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher;

/// <summary>
/// Provides a mechanism for sending posts to a variety of external resources
/// Such as IRC, Discord, Twitter, etc via a collection of
/// <see cref="IPostDistributor"/> instances that will deliver posts to specific resources.
/// </summary>
public class ExternalMediaPublisher // DI as a singleton, pass in a hardcoded list of IMessagingProvider implementations, config drive which implementations to use
{
	private readonly string _baseUrl; // The site base url, will be combined to relative links to provide absolute links to distributors

	public ExternalMediaPublisher(AppSettings appSettings, IEnumerable<IPostDistributor> providers)
	{
		_baseUrl = appSettings.BaseUrl.TrimEnd('/');
		Providers = providers.ToList();
	}

	// Calling code will likely not know or care the list, but doesn't hurt to expose it
	public IEnumerable<IPostDistributor> Providers { get; }

	public async Task Send(IPostable message)
	{
		if (message == null)
		{
			throw new ArgumentException($"{nameof(message)} can not be null");
		}

		var providers = Providers.Where(p => p.Types.Contains(message.Type));
		await Task.WhenAll(providers.Select(p => p.Post(message)));
	}

	public string ToAbsolute(string relativeLink)
	{
		return !string.IsNullOrWhiteSpace(relativeLink)
			? $"{_baseUrl}/{relativeLink.TrimStart('/')}"
			: "";
	}
}

public static class ExternalMediaPublisherExtensions
{
	public static async Task SendUserFile(this ExternalMediaPublisher publisher, bool unlisted, string title, string relativeLink, string body)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = unlisted
				? PostType.Administrative
				: PostType.General,
			Group = PostGroups.UserFiles,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task AnnounceSubmission(this ExternalMediaPublisher publisher, string title, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "New Submission!",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = $"New Submission! Go and see {title}",
			Body = "",
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendSubmissionEdit(this ExternalMediaPublisher publisher, string title, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.General,
			Group = PostGroups.Submission,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task AnnouncePublication(this ExternalMediaPublisher publisher, string title, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "New Movie Published!",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = $"New movie published! Go and see {title}",
			Body = "",
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendPublicationEdit(this ExternalMediaPublisher publisher, string title, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.General,
			Group = PostGroups.Publication,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task AnnounceForum(this ExternalMediaPublisher publisher, string title, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "News Post!",
			Type = PostType.Announcement,
			Group = PostGroups.Forum,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendForum(this ExternalMediaPublisher publisher, bool restricted, string title, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Type = restricted
				? PostType.Administrative
				: PostType.General,
			Group = PostGroups.Forum,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendGeneralWiki(this ExternalMediaPublisher publisher, string title, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.General,
			Group = PostGroups.Wiki,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendUserManagement(this ExternalMediaPublisher publisher, string title, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.Administrative,
			Group = PostGroups.UserManagement,
			Title = title,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}
}
