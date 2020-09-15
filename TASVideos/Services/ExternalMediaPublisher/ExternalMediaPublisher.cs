using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;

namespace TASVideos.Services.ExternalMediaPublisher
{
	/// <summary>
	/// Provides a mechanism for sending posts to a variety of external resources
	/// Such as IRC, Discord, Twitter, etc via a collection of 
	/// <see cref="IPostDistributor"/> instances that will deliver posts to
	/// specific resources
	/// </summary>
	public class ExternalMediaPublisher // DI as a singleton, pass in a hardcoded list of IMessagingProvider implementations, config drive which implementations to use
	{
		private readonly string _baseUrl; // The site base url, will be combined to relative links to provide absolute links to distributors

		public ExternalMediaPublisher(IOptions<AppSettings> appSettings, IEnumerable<IPostDistributor> providers)
		{
			_baseUrl = appSettings.Value.BaseUrl.TrimEnd('/');
			Providers = providers.ToList();
		}

		// Calling code will likely not know or care the list, but doesn't hurt to expose it
		public IEnumerable<IPostDistributor> Providers { get; }

		public void Send(IPostable message)
		{
			if (message == null)
			{
				throw new ArgumentException($"{nameof(message)} can not be null");
			}

			var providers = Providers.Where(p => p.Types.Contains(message.Type));
			foreach (var provider in providers)
			{
				provider.Post(message);
			}
		}

		public string ToAbsolute(string relativeLink)
		{
			return !string.IsNullOrWhiteSpace(relativeLink)
				? Path.Combine(_baseUrl, relativeLink.TrimStart('/'))
				: "";
		}
	}

	public static class ExternalMediaPublisherExtensions
	{
		public static void SendUserFile(this ExternalMediaPublisher publisher, string title, string relativeLink, string body = "", string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.General,
				Group = PostGroups.UserFiles,
				Title = title,
				Body = body,
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void AnnounceSubmission(this ExternalMediaPublisher publisher, string title, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.Announcement,
				Group = PostGroups.Submission,
				Title = $"New Submission! Go and see {title}",
				Body = "",
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void SendSubmissionEdit(this ExternalMediaPublisher publisher, string title, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.General,
				Group = PostGroups.Submission,
				Title = title,
				Body = "",
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void AnnouncePublication(this ExternalMediaPublisher publisher, string title, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.Announcement,
				Group = PostGroups.Submission,
				Title = $"New movie published! Go and see {title}",
				Body = "",
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void SendPublicationEdit (this ExternalMediaPublisher publisher, string title, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.General,
				Group = PostGroups.Submission,
				Title = title,
				Body = "",
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void SendForum(this ExternalMediaPublisher publisher, bool restricted, string title, string body, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = restricted
					? PostType.Administrative
					: PostType.General,
				Group = PostGroups.Forum,
				Title = title,
				Body = body,
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void SendGeneralWiki(this ExternalMediaPublisher publisher, string title, string body, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.General,
				Group = PostGroups.Wiki,
				Title = title,
				Body = body,
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}

		public static void SendUserManagement(this ExternalMediaPublisher publisher, string title, string body, string relativeLink, string user = "")
		{
			publisher.Send(new Post
			{
				Type = PostType.Administrative,
				Group = PostGroups.UserManagement,
				Title = title,
				Body = body,
				Link = publisher.ToAbsolute(relativeLink),
				User = user
			});
		}
	}
}
