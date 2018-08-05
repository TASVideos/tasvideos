using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
    /// <summary>
	/// A <see cref="IPostDistributor"/> implementation that simply logs
	/// a post to the console
	/// </summary>
	public class ConsoleDistributor : IPostDistributor
	{
		private static readonly IEnumerable<PostType> PostTypes = Enum
			.GetValues(typeof(PostType))
			.OfType<PostType>()
			.ToList();

		public IEnumerable<PostType> Types => PostTypes;

		public void Post(IPostable post)
		{
			Console.WriteLine($"New {post.Type} message recieved\n{post.Title}\n{post.Body}\nLink:{post.Link}\nGroup:{post.Group}");
		}
	}
}
