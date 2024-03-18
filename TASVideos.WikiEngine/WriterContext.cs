namespace TASVideos.WikiEngine.AST;

/// <summary>
/// Used internally by nodes to assist them in writing output.
/// </summary>
public class WriterContext(IWriterHelper helper)
{
	private readonly List<KeyValuePair<Regex, string>> _tableAttributeRunners = [];

	public IWriterHelper Helper { get; } = helper;

	/// <summary>
	/// Adds a table style filter expression for later use in table cells.
	/// </summary>
	/// <param name="pp">The raw parameter text from the markup.</param>
	public bool AddTdStyleFilter(IReadOnlyDictionary<string, string> pp)
	{
		var regex = pp.GetValueOrDefault("pattern");
		var style = pp.GetValueOrDefault("style");
		if (string.IsNullOrWhiteSpace(regex) || string.IsNullOrWhiteSpace(style))
		{
			return false;
		}

		try
		{
			// TODO: What's actually going on with these @s?
			if (regex[0] == '@')
			{
				regex = regex[1..];
			}

			if (regex[^1] == '@')
			{
				regex = regex[..^1];
			}

			var r = new Regex(regex, RegexOptions.None, TimeSpan.FromSeconds(1));
			_tableAttributeRunners.Add(new KeyValuePair<Regex, string>(r, style));
		}
		catch
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Run all existing td style filters.
	/// </summary>
	/// <param name="text">The raw text to evaluate against the style filters.</param>
	/// <returns>A style attribute value, or null if no filters matched.</returns>
	public string? RunTdStyleFilters(string text)
	{
		foreach (var (key, value) in _tableAttributeRunners)
		{
			if (key.Match(text).Success)
			{
				return value;
			}
		}

		return null;
	}
}
