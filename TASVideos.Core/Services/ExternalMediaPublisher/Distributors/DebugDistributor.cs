using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors
{
	/// <summary>
	/// A <see cref="IPostDistributor"/> implementation that simply logs a post to the console.
	/// </summary>
	public class ConsoleDistributor : IPostDistributor
	{
		private readonly ILogger _logger;

		private static readonly IEnumerable<PostType> PostTypes = Enum
			.GetValues(typeof(PostType))
			.OfType<PostType>()
			.ToList();

		public ConsoleDistributor(ILogger<ConsoleDistributor> logger)
		{
			_logger = logger;
		}

		public IEnumerable<PostType> Types => PostTypes;

		public void Post(IPostable post)
		{
			_logger.LogInformation($"New {post.Type} message recieved\n{post.Title}\n{post.Body}\nLink:{post.Link}\nGroup:{post.Group}\n{post.User}");
		}
	}
}
