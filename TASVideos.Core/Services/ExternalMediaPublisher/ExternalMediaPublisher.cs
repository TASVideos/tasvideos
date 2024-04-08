using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher;

/// <summary>
/// Provides a mechanism for sending posts to a variety of external resources
/// Such as IRC, Discord, etc. via a collection of
/// <see cref="IPostDistributor"/> instances that will deliver posts to specific resources.
/// </summary>
public class
	ExternalMediaPublisher // DI as a singleton, pass in a hardcoded list of IMessagingProvider implementations, config drive which implementations to use
	(AppSettings appSettings, IEnumerable<IPostDistributor> providers)
{
	private readonly string _baseUrl = appSettings.BaseUrl.TrimEnd('/'); // The site base url, will be combined to relative links to provide absolute links to distributors

	// Calling code will likely not know or care the list, but doesn't hurt to expose it
	public IEnumerable<IPostDistributor> Providers { get; } = providers.ToList();

	public async Task Send(IPostable message)
	{
		if (message is null)
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
	public static async Task SendUserFile(this ExternalMediaPublisher publisher, bool unlisted, string formattedTitle, long id, string fileTitle)
	{
		string unformattedTitle = Unformat(formattedTitle);
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = unlisted
				? PostType.Administrative
				: PostType.General,
			Group = PostGroups.UserFiles,
			Title = unformattedTitle,
			FormattedTitle = formattedTitle,
			Body = fileTitle,
			Link = publisher.ToAbsolute($"UserFiles/Info/{id}")
		});
	}

	public static async Task AnnounceNewSubmission(this ExternalMediaPublisher publisher, Submission submission)
	{
		await publisher.Send(new Post
		{
			Announcement = "New Submission!",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = $"New Submission! Go and see {submission.Title}",
			FormattedTitle = $"New Submission! Go and see [{submission.Title}]({{0}})",
			Body = "",
			Link = publisher.ToAbsolute($"{submission.Id}S")
		});
	}

	public static async Task SendSubmissionEdit(this ExternalMediaPublisher publisher, int subId, string formattedTitle, string body)
	{
		await publisher.Send(new Post
		{
			Type = PostType.General,
			Group = PostGroups.Submission,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = body,
			Link = $"{subId}S"
		});
	}

	public static async Task SendDeprecation(this ExternalMediaPublisher publisher, string formattedTitle)
	{
		await publisher.Send(new Post
		{
			Type = PostType.General,
			Group = PostGroups.Submission,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle
		});
	}

	public static async Task AnnounceNewPublication(this ExternalMediaPublisher publisher, Publication publication)
	{
		await publisher.Send(new Post
		{
			Announcement = "New Movie Published!",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = $"New movie published! Go and see {publication.Title}",
			FormattedTitle = $"New movie published! Go and see [{publication.Title}]({{0}})",
			Body = "",
			Link = publisher.ToAbsolute($"{publication.Id}M")
		});
	}

	public static async Task AnnounceUnpublish(this ExternalMediaPublisher publisher, string publicationTitle, int id, string reason)
	{
		await publisher.Send(new Post
		{
			Announcement = $"{publicationTitle} REMOVED",
			Type = PostType.Announcement,
			Group = PostGroups.Publication,
			Title = "Publication Removed",
			FormattedTitle = "[Publication]({0}) Removed",
			Body = reason,
			Link = publisher.ToAbsolute($"{id}M")
		});
	}

	public static async Task AnnounceSubmissionDelete(this ExternalMediaPublisher publisher, string submissionTitle, int id)
	{
		await publisher.Send(new Post
		{
			Announcement = $"{submissionTitle} REMOVED",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = "Submission Removed",
			FormattedTitle = "[Submission]({0}) Removed",
			Link = publisher.ToAbsolute($"{id}S")
		});
	}

	public static async Task SendPublicationEdit(this ExternalMediaPublisher publisher, string userName, int publicationId, string body)
	{
		var title = $"{publicationId}M edited by {userName}";
		var formattedTitle = $"[{publicationId}M]({{0}}) edited by {userName}";
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.General,
			Group = PostGroups.Publication,
			Title = title,
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute($"{publicationId}M")
		});
	}

	public static async Task SendPublicationClassChange(this ExternalMediaPublisher publisher, int id, string pubTitle, string userName, string oldClass, string newClass)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.General,
			Group = PostGroups.Publication,
			Title = $"{id}M Class changed from {oldClass} to {newClass} by {userName}",
			FormattedTitle = $"[{id}M]({{0}}) Class changed from {oldClass} to {newClass} by {userName}",
			Body = pubTitle,
			Link = publisher.ToAbsolute($"{id}M")
		});
	}

	public static async Task AnnounceNewsPost(this ExternalMediaPublisher publisher, string formattedTitle, string body, int postId)
	{
		await publisher.Send(new Post
		{
			Announcement = "News Post!",
			Type = PostType.Announcement,
			Group = PostGroups.Forum,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = body,
			Link = $"Forum/Posts/{postId}"
		});
	}

	public static async Task SendForum(this ExternalMediaPublisher publisher, bool restricted, string formattedTitle, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Type = restricted
				? PostType.Administrative
				: PostType.General,
			Group = PostGroups.Forum,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendGeneralWiki(this ExternalMediaPublisher publisher, string title, string formattedTitle, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.General,
			Group = PostGroups.Wiki,
			Title = title,
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendUserManagement(this ExternalMediaPublisher publisher, string title, string formattedTitle, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Announcement = "",
			Type = PostType.Administrative,
			Group = PostGroups.UserManagement,
			Title = title,
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	public static async Task SendGameManagement(this ExternalMediaPublisher publisher, string title, string formattedTitle, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Type = PostType.General,
			Group = PostGroups.Game,
			Title = title,
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	// formatted: New [user file]({0}) uploaded by
	// unformatted: New user file uploaded by
	private static string Unformat(string formattedTitle)
	{
		return formattedTitle.Replace("[", "").Replace("]", "").Replace("{0}", "");
	}
}
