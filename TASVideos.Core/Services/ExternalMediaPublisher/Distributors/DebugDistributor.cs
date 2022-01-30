using Microsoft.Extensions.Logging;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

/// <summary>
/// A <see cref="IPostDistributor"/> implementation that simply logs a post.
/// </summary>
public class LogDistributor : IPostDistributor
{
	private readonly ILogger _logger;

	private static readonly IEnumerable<PostType> PostTypes = Enum
		.GetValues(typeof(PostType))
		.OfType<PostType>()
		.ToList();

	public LogDistributor(ILogger<LogDistributor> logger)
	{
		_logger = logger;
	}

	public IEnumerable<PostType> Types => PostTypes;

	public async Task Post(IPostable post)
	{
		await Task.Run(() => _logger.LogInformation($"New {post.Type} message recieved\n{post.Title}\n{post.Body}\nLink:{post.Link}\nGroup:{post.Group}\n{post.User}"));
	}
}
