namespace TASVideos.Data.Helpers;

public static class SubmissionHelper
{
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
		=> IsRawNumberedLink(link, LinkConstants.SubmissionWikiPage);

	public static int? IsRawPublicationLink(string link)
		=> IsRawNumberedLink(link, LinkConstants.PublicationWikiPage);

	public static int? IsRawGamePageLink(string link)
		=> IsRawNumberedLink(link, LinkConstants.GameWikiPage);

	public static bool JudgeIsClaiming(SubmissionStatus oldS, SubmissionStatus newS)
		=> oldS != SubmissionStatus.JudgingUnderWay && newS == SubmissionStatus.JudgingUnderWay;

	public static bool JudgeIsUnclaiming(SubmissionStatus newS)
		=> newS == SubmissionStatus.New;

	public static bool PublisherIsClaiming(SubmissionStatus oldS, SubmissionStatus newS)
		=> oldS != SubmissionStatus.PublicationUnderway && newS == SubmissionStatus.PublicationUnderway;

	public static bool PublisherIsUnclaiming(SubmissionStatus oldS, SubmissionStatus newS)
		=> oldS == SubmissionStatus.PublicationUnderway && newS == SubmissionStatus.Accepted;

	private static int? IsNumberedLink(string link, string suffix)
	{
		link = link.Trim('/');
		if (!link.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		var numberText = link.AsSpan(0, link.Length - 1);
		if (int.TryParse(numberText, out int id))
		{
			return id;
		}

		return null;
	}

	private static int? IsRawNumberedLink(string link, string prefix)
	{
		link = link.Trim('/');
		if (!link.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		var numberText = link.AsSpan(prefix.Length, link.Length - prefix.Length);
		if (int.TryParse(numberText, out int id))
		{
			return id;
		}

		return null;
	}
}
