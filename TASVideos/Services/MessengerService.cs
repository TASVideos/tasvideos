using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Services.Messenger
{
	public enum MessageType
	{
		Critical, // Hihgly time sensitive administrative alerts, used for emergency situations
		Administrative, // Not visible to general public, should only go to restricted channels available to staff/admin
		Announcement, // Public announcement
		General, // Only reported in general public areas and not announceable media, ex: would be reported in irc, but would not be tweeted
		Log // Minimally important information only useful for logging purposes
	}

	// Interfaced so calling code has flexibility, for isntance an existing Dto may have an interface tacked on it with dericed properties
	public interface IMessage
	{
		string Title { get; }
		string Link { get; } // A link that will direct a user to more detailed enformation
		string Details { get; }
		string Group { get; } // Arbitruary identifier for grouping types of message, this may or may nto be used by a service provider
		MessageType Type { get; }
	}

	public class Message : IMessage
	{
		public string Title { get; set; }
		public string Link { get; set; }
		public string Details { get; set; }
		public string Group { get; set; }
		public MessageType Type { get; set; }
	}

	public class MessengerService // DI as a singleton, pass in a hardcoded list of IMessagingProvider implemetnations, config drive which implementations to use
	{
		public MessengerService(IEnumerable<IMessagingProvider> providers)
		{
			Providers = providers.ToList();
		}

		// Calling code will likely not know or care the list, but doesn't hurt to expose it
		public IEnumerable<IMessagingProvider> Providers { get; }

		public void Send(IMessage message)
		{
			if (message == null)
			{
				throw new ArgumentException($"{nameof(message)} can not be null");
			}

			var providers = Providers.Where(p => p.Types.Contains(message.Type));
			foreach (var provider in providers)
			{
				provider.SendAsync(message);
			}
		}
	}

	// Implementations:
	// IrcProvider
	// PrivateIrcProvider
	// DiscordProvider
	// LogProvider
	// DbProvider
	// RssProvider
	// TwitterProvider
	// Implementations can config drive necessary paramaeters, for instance, Discord server path, irc server and channel, log in credentials etc
	public interface IMessagingProvider
	{
		IEnumerable<MessageType> Types { get;  } // a list of all types that this provider will respond to, if a type is not here it will not send messages of that type
		void SendAsync(IMessage message); // TODO: Fire and forget, should it say Async? should it return a Task?
	}
}
