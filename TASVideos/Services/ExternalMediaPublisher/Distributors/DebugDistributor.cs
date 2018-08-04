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

		public void Post(IPostable message)
		{
			Console.WriteLine($"New {message.Type} message recieved\n{message.Title}\n{message.Body}\nLink:{message.Link}\nGroup:{message.Group}");
		}
	}
}
