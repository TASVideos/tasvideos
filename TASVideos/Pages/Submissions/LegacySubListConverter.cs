using TASVideos.Data.Entity;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions;

/// <summary>
/// Converts legacy query string parameters from Subs-List to a <see cref="SubmissionSearchRequest"/> model
/// Does not support user filtering since the user is the wiki site id which we do not keep
/// </summary>
public static class LegacySubListConverter
{
	public static SubmissionSearchRequest? ToSearchRequest(string? query)
	{
		var tokens = query.ToTokens();

		if (!tokens.Any())
		{
			return null;
		}

		var request = new SubmissionSearchRequest();

		var statuses = new List<SubmissionStatus>();
		foreach ((var key, SubmissionStatus value) in StatusTokenMapping)
		{
			if (tokens.Any(t => t == key))
			{
				statuses.Add(value);
			}
		}

		if (statuses.Any())
		{
			request.StatusFilter = statuses;
		}

		var years = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1);
		request.Years = years.Where(y => tokens.Contains("y" + y));

		return request;
	}

	private static readonly Dictionary<string, SubmissionStatus> StatusTokenMapping = new()
	{
		["new"] = SubmissionStatus.New,
		["can"] = SubmissionStatus.Cancelled,
		["inf"] = SubmissionStatus.NeedsMoreInfo,
		["del"] = SubmissionStatus.Delayed,
		["jud"] = SubmissionStatus.JudgingUnderWay,
		["acc"] = SubmissionStatus.Accepted,
		["und"] = SubmissionStatus.PublicationUnderway,
		["pub"] = SubmissionStatus.Published,
		["rej"] = SubmissionStatus.Rejected
	};
}
