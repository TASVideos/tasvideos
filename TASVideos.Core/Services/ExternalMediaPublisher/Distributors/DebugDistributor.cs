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
		if (_logger.IsEnabled(LogLevel.Information))
		{
			await Task.Run(() => _logger.LogInformation(
				"New {post.Type} message recieved\n{announcement}\n{title}\n{body}\nLink:{link}\nGroup:{group}\n{user}",
				post.Type,
				post.Announcement,
				post.Title,
				post.Body,
				post.Link,
				post.Group,
				post.User));
		}
	}
}
