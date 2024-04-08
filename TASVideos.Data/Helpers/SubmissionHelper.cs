namespace TASVideos.Data.Helpers;

public static class SubmissionHelper
{
	private static int? IsNumberedLink(string? link, string suffix)
	{
		if (link != null && link.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
		{
			var rooted = link.StartsWith('/');
			var numberText = link.AsSpan(rooted ? 1 : 0, link.Length - (rooted ? 2 : 1));
			if (int.TryParse(numberText, out int id))
			{
				return id;
			}
		}

		return null;
	}

	private static int? IsRawNumberedLink(string? link, string prefix)
	{
		link = link?.Trim('/');
		if (link != null && link.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
		{
			var numberText = link.Replace(prefix, "");
			if (int.TryParse(numberText, out int id))
			{
				return id;
			}
		}

		return null;
	}

	/// <summary>
	/// Determines if the link is in the form of valid submission link ex: 100S.
	/// </summary>
	/// <returns>The id of the submission if it is a valid link, else null.</returns>
	public static int? IsSubmissionLink(string link) => IsNumberedLink(link, "S");

	/// <summary>
	/// Determines if the link is in the form of valid movie link ex: 100M.
	/// </summary>
	/// <returns>The id of the movie if it is a valid link, else null.</returns>
	public static int? IsPublicationLink(string link) => IsNumberedLink(link, "M");

	/// <summary>
	/// Determines if the link is in the form of valid game page link ex: 100G.
	/// </summary>
	/// <returns>The id of the movie if it is a valid link, else null.</returns>
	public static int? IsGamePageLink(string link) => IsNumberedLink(link, "G");

	public static int? IsRawSubmissionLink(string link)
		=> IsRawNumberedLink(link, "InternalSystem/SubmissionContent/S");

	public static int? IsRawPublicationLink(string link)
		=> IsRawNumberedLink(link, "InternalSystem/PublicationContent/M");

	public static int? IsRawGamePageLink(string link)
		=> IsRawNumberedLink(link, "InternalSystem/GameContent/G");
}
