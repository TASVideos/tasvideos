namespace TASVideos.Services.ExternalMediaPublisher
{
    /// <summary>
	/// Represents a post to be sent to the <see cref="TASVideos.Services.Messenger"/>
	/// to then be sent to any registered <see cref="IPostDistributor"/>s
	/// </summary>
	public interface IPostable
	{
		/// <summary>
		/// The post title
		/// </summary>
		string Title { get; }

		/// <summary>
		/// A link that will direct a user to the resource or to more detailed information
		/// </summary>
		string Link { get; }

		/// <summary>
		/// The body of the post
		/// </summary>
		string Body { get; }

		/// <summary>
		/// Arbitrary identifier for grouping types of post,
		/// this may or may not be used by a service provider
		/// </summary>
		string Group { get; } 

		/// <summary>
		/// The type of the message, depending on the message
		/// A <see cref="IPostDistributor"/> may or may not respond to it
		/// </summary>
		PostType Type { get; }
	}

	/// <summary>
	/// A default implementation of the <see cref="IPostable"/> interface
	/// </summary>
	public class Post : IPostable
	{
		public string Title { get; set; }
		public string Link { get; set; }
		public string Body { get; set; }
		public string Group { get; set; }
		public PostType Type { get; set; }
	}
}
