namespace TASVideos.WikiEngine.AST;

/// <summary>
/// Provides helpers that the wiki engine needs to render page results
/// </summary>
public interface IWriterHelper
{
	/// <summary>
	/// Check the condition for one of the wiki language's conditional markups
	/// </summary>
	/// <param name="condition">The condition; eg `CanEditPages` or `!CanJudgeMovies`</param>
	/// <returns>The value of the condition for the current user context.</returns>
	bool CheckCondition(string condition);

	/// <summary>
	/// Run a ViewComponent ("module" in wiki lingo)
	/// </summary>
	/// <param name="w">The stream that the module should output its markup results to.</param>
	/// <param name="name">The name of the module.</param>
	/// <param name="pp">The module's parsed parameters.</param>
	Task RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp);

	/// <summary>
	/// Converts a relative URL into an absolute one.  Can leave the URL alone if relative links
	/// are desired.
	/// </summary>
	string AbsoluteUrl(string url);
}

/// <summary>
/// A fake IWriterHelper which can give "good enough" results if a static context is needed.
/// </summary>
public class NullWriterHelper : IWriterHelper
{
	public bool CheckCondition(string condition)
	{
		return false;
	}

	public Task RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
	{
		return Task.CompletedTask;
	}

	public string AbsoluteUrl(string url)
	{
		return url;
	}

	private NullWriterHelper()
	{
	}

	public static readonly NullWriterHelper Instance = new();
}
