using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Settings;

namespace TASVideos.Services;

/// <summary>
/// Provides a mechanism for sending posts to a variety of external resources
/// Such as IRC, Discord, etc. via a collection of
/// <see cref="IPostDistributor"/> instances that will deliver posts to specific resources.
/// </summary>
public interface IExternalMediaPublisher
{
	Task Send(IPostable message, bool force = false);
	string ToAbsolute(string relativeLink);
}

internal class ExternalMediaPublisher(AppSettings appSettings, IEnumerable<IPostDistributor> providers, IHttpContextAccessor httpContextAccessor) : IExternalMediaPublisher
{
	private readonly string _baseUrl = appSettings.BaseUrl.TrimEnd('/');

	private IEnumerable<IPostDistributor> Providers { get; } = [.. providers];

	public async Task Send(IPostable message, bool force = false)
	{
		if (!force)
		{
			var isMinorEdit = httpContextAccessor.HttpContext?.Request.MinorEdit() ?? false;
			if (isMinorEdit)
			{
				return;
			}
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
	public static async Task SendMessage(this IExternalMediaPublisher publisher, string group, string message, string body = "")
	{
		await publisher.Send(new Post
		{
			Group = group,
			Title = message,
			Body = body
		});
	}

	public static async Task SendAdminMessage(this IExternalMediaPublisher publisher, string group, string message)
	{
		await publisher.Send(new Post
		{
			Type = PostType.Administrative,
			Group = group,
			Title = message,
		});
	}

	public static async Task SendUserFile(this IExternalMediaPublisher publisher, bool unlisted, string formattedTitle, long id, string fileTitle)
	{
		string unformattedTitle = Unformat(formattedTitle);
		await publisher.Send(new Post
		{
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

	public static async Task AnnounceNewSubmission(this IExternalMediaPublisher publisher, Submission submission, byte[]? imageData = null, string? imageMimeType = null, int? imageWidth = null, int? imageHeight = null)
	{
		await publisher.Send(new Post
		{
			Announcement = "New Submission!",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = $"New Submission! Go and see {submission.Title}",
			FormattedTitle = $"New Submission! Go and see [{submission.Title}]({{0}})",
			Body = "",
			Link = publisher.ToAbsolute($"{submission.Id}S"),
			ImageData = imageData,
			ImageMimeType = imageMimeType,
			ImageWidth = imageWidth,
			ImageHeight = imageHeight
		});
	}

	public static async Task SendSubmissionEdit(this IExternalMediaPublisher publisher, int subId, string formattedTitle, string body, bool force = false)
	{
		await publisher.Send(
			new Post
			{
				Group = PostGroups.Submission,
				Title = Unformat(formattedTitle),
				FormattedTitle = formattedTitle,
				Body = body,
				Link = publisher.ToAbsolute($"{subId}S")
			},
			force);
	}

	public static async Task SendDeprecation(this IExternalMediaPublisher publisher, string formattedTitle)
	{
		await publisher.Send(new Post
		{
			Group = PostGroups.Submission,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle
		});
	}

	public static async Task AnnounceNewPublication(this IExternalMediaPublisher publisher, Publication publication, byte[]? imageData = null, string? imageMimeType = null, int? imageWidth = null, int? imageHeight = null)
	{
		await publisher.Send(new Post
		{
			Announcement = "New Movie Published!",
			Type = PostType.Announcement,
			Group = PostGroups.Submission,
			Title = $"New movie published! Go and see {publication.Title}",
			FormattedTitle = $"New movie published! Go and see [{publication.Title}]({{0}})",
			Body = "",
			Link = publisher.ToAbsolute($"{publication.Id}M"),
			ImageData = imageData,
			ImageMimeType = imageMimeType,
			ImageWidth = imageWidth,
			ImageHeight = imageHeight
		});
	}

	public static async Task AnnounceUnpublish(this IExternalMediaPublisher publisher, string publicationTitle, int id, string reason)
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

	public static async Task AnnounceSubmissionDelete(this IExternalMediaPublisher publisher, string submissionTitle, int id)
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

	public static async Task SendPublicationEdit(this IExternalMediaPublisher publisher, string userName, int publicationId, string body)
	{
		var title = $"{publicationId}M edited by {userName}";
		var formattedTitle = $"[{publicationId}M]({{0}}) edited by {userName}";
		await publisher.Send(new Post
		{
			Group = PostGroups.Publication,
			Title = title,
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute($"{publicationId}M")
		});
	}

	public static async Task SendPublicationClassChange(this IExternalMediaPublisher publisher, int id, string pubTitle, string userName, string oldClass, string newClass)
	{
		await publisher.Send(new Post
		{
			Group = PostGroups.Publication,
			Title = $"{id}M Class changed from {oldClass} to {newClass} by {userName}",
			FormattedTitle = $"[{id}M]({{0}}) Class changed from {oldClass} to {newClass} by {userName}",
			Body = pubTitle,
			Link = publisher.ToAbsolute($"{id}M")
		});
	}

	public static async Task AnnounceNewsPost(this IExternalMediaPublisher publisher, string formattedTitle, string body, int postId)
	{
		await publisher.Send(new Post
		{
			Announcement = "News Post!",
			Type = PostType.Announcement,
			Group = PostGroups.Forum,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute($"Forum/Posts/{postId}")
		});
	}

	public static async Task SendForum(this IExternalMediaPublisher publisher, bool restricted, string formattedTitle, string body, string relativeLink)
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

	public static async Task SendWiki(this IExternalMediaPublisher publisher, string formattedTitle, string body, string path, bool force = false)
	{
		await publisher.Send(
		new Post
		{
			Group = PostGroups.Wiki,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = body,
			Link = !string.IsNullOrWhiteSpace(path)
				? publisher.ToAbsolute(WikiHelper.EscapeUserName(path))
				: ""
		},
		force);
	}

	public static async Task SendUserManagement(this IExternalMediaPublisher publisher, string formattedTitle, string userName)
	{
		await publisher.Send(new Post
		{
			Type = PostType.Administrative,
			Group = PostGroups.UserManagement,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Link = publisher.ToAbsolute($"Users/Profile/{Uri.EscapeDataString(userName)}")
		});
	}

	public static async Task SendRoleManagement(this IExternalMediaPublisher publisher, string formattedTitle, string roleName)
	{
		await publisher.Send(new Post
		{
			Type = PostType.Administrative,
			Group = PostGroups.UserManagement,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = "",
			Link = publisher.ToAbsolute($"Roles/{roleName}")
		});
	}

	public static async Task SendGameManagement(this IExternalMediaPublisher publisher, string formattedTitle, string body, string relativeLink)
	{
		await publisher.Send(new Post
		{
			Group = PostGroups.Game,
			Title = Unformat(formattedTitle),
			FormattedTitle = formattedTitle,
			Body = body,
			Link = publisher.ToAbsolute(relativeLink)
		});
	}

	// formatted: New [user file]({0}) uploaded by
	// unformatted: New user file uploaded by
	private static string Unformat(string formattedTitle)
		=> formattedTitle.Replace("[", "").Replace("]", "").Replace("({0})", "");
}
