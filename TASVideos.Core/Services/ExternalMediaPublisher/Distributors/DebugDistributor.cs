using Microsoft.Extensions.Logging;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

/// <summary>
/// A <see cref="IPostDistributor"/> implementation that simply logs a post.
/// </summary>
public class LogDistributor(ILogger<LogDistributor> logger) : IPostDistributor
{
	private static readonly IEnumerable<PostType> PostTypes = Enum.GetValues<PostType>().ToList();

	public IEnumerable<PostType> Types => PostTypes;

	public async Task Post(IPostable post)
	{
		if (logger.IsEnabled(LogLevel.Information))
		{
			await Task.Run(() => logger.LogInformation(
				"New {post.Type} message received\n{announcement}\n{title}\n{body}\nLink:{link}\nGroup:{group}\n{user}",
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
