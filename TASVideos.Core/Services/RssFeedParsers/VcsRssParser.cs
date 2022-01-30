using TASVideos.Core.Services.RssFeedParsers.Github;

namespace TASVideos.Core.Services.RssFeedParsers;

public interface IVcsRssParser
{
	bool IsSupportedType(string type);
	IEnumerable<CommitEntry> Parse(string type, string xml);
}

internal class VcsRssParser : IVcsRssParser
{
	private const int MaxRecords = 10;
	private readonly ICacheService _cache;

	public VcsRssParser(ICacheService cache)
	{
		_cache = cache;
	}

	public IEnumerable<CommitEntry> Parse(string type, string xml)
	{
		if (!IsSupportedType(type))
		{
			throw new InvalidOperationException($" is not a valid {nameof(type)}");
		}

		if (_cache.TryGetValue(xml, out IEnumerable<CommitEntry> entries))
		{
			return entries;
		}

		entries = type switch
		{
			"atom" => GithubFeed.Parse(xml, MaxRecords),
			_ => throw new NotImplementedException($"{nameof(type)} {type} is not supported.")
		};

		_cache.Set(xml, entries);
		return entries;
	}

	public bool IsSupportedType(string type)
		=> new[] { "atom" }.Contains(type);
}

public record CommitEntry(
	string Author,
	string At,
	string Message,
	string Link);
