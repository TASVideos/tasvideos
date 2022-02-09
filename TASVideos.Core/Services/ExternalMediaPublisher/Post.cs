﻿namespace TASVideos.Core.Services.ExternalMediaPublisher;

/// <summary>
/// Represents a post to be sent to the <see cref="ExternalMediaPublisher"/>
/// to then be sent to any registered <see cref="IPostDistributor"/>s
/// </summary>
public interface IPostable
{
	/// <summary>
	/// Gets the post announcement message
	/// </summary>
	string Announcement { get; }

	/// <summary>
	/// Gets the post title
	/// </summary>
	string Title { get; }

	/// <summary>
	/// Gets a link that will direct a user to the resource or to more detailed information
	/// </summary>
	string Link { get; }

	/// <summary>
	/// Gets the body of the post
	/// </summary>
	string Body { get; }

	/// <summary>
	/// Gets an arbitrary identifier for grouping types of post,
	/// this may or may not be used by a service provider
	/// </summary>
	string Group { get; }

	/// <summary>
	/// Gets the type of the message, depending on the message
	/// A <see cref="IPostDistributor"/> may or may not respond to it
	/// </summary>
	PostType Type { get; }

	/// <summary>
	/// Gets the person that posted the message.
	/// </summary>
	string User { get; }
}

/// <summary>
/// A default implementation of the <see cref="IPostable"/> interface
/// </summary>
public class Post : IPostable
{
	public string Announcement { get; set; } = "";
	public string Title { get; init; } = "";
	public string Link { get; init; } = "";
	public string Body { get; init; } = "";
	public string Group { get; init; } = "";
	public string User { get; init; } = "";
	public PostType Type { get; init; }
}
