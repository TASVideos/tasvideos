using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Constants;

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
		public ExternalMediaPublisher(IEnumerable<IPostDistributor> providers)
		{
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
	}

	public static class ExternalMediaPublisherExtensions
	{
		public static void AnnounceSubmission(this ExternalMediaPublisher publisher, string title, string link)
		{
			publisher.Send(new Post
			{
				Type = PostType.Announcement,
				Group = PostGroups.Submission,
				Title = $"New Submission! Go and see {title}",
				Body = "",
				Link = link
			});
		}

		public static void AnnouncePublication(this ExternalMediaPublisher publisher, string title, string link)
		{
			publisher.Send(new Post
			{
				Type = PostType.Announcement,
				Group = PostGroups.Submission,
				Title = $"New movie published! Go and see {title}",
				Body = "",
				Link = link
			});
		}

		public static void SendForum(this ExternalMediaPublisher publisher, bool restricted, string title, string body, string link)
		{
			publisher.Send(new Post
			{
				Type = restricted
					? PostType.Administrative
					: PostType.General,
				Group = PostGroups.Forum,
				Title = title,
				Body = body,
				Link = link
			});
		}

		public static void SendAdminForum(this ExternalMediaPublisher publisher, string title, string body, string link)
		{
			publisher.Send(new Post
			{
				Type = PostType.Administrative,
				Group = PostGroups.Forum,
				Title = title,
				Body = body,
				Link = link
			});
		}

		public static void SendGeneralWiki(this ExternalMediaPublisher publisher, string title, string body, string link)
		{
			publisher.Send(new Post
			{
				Type = PostType.General,
				Group = PostGroups.Wiki,
				Title = title,
				Body = body,
				Link = link
			});
		}
	}
}
