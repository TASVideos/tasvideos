using Microsoft.AspNetCore.Mvc.Rendering;

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

	public static int SubmissionId(string link)
	{
		if (IsSubmissionLink(link) is null)
		{
			return int.Parse(link.SplitWithEmpty("/").Last().Replace("S", ""));
		}

		return 0;
	}

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

	public static readonly SelectListItem[] GameVersionOptions =
	{
		new() { Text = "unknown", Value = "unknown" },
		new() { Text = "unknown v1.0", Value = "unknown v1.0" },
		new() { Text = "unknown v1.1", Value = "unknown v1.1" },
		new() { Text = "unknown,r0", Value = "unknown,r0" },
		new() { Text = "unknown,r1", Value = "unknown,r1" },
		new() { Text = "unknown,r2", Value = "unknown,r2" },
		new() { Text = "unknown PRG0", Value = "unknown PRG0" },
		new() { Text = "unknown PRG1", Value = "unknown PRG1" },
		new() { Text = "unknown PRG2", Value = "unknown PRG2" },
		new() { Text = "any", Value = "any" },
		new() { Text = "any v1.0", Value = "any v1.0" },
		new() { Text = "any v1.1", Value = "any v1.1" },
		new() { Text = "any,r0", Value = "any,r0" },
		new() { Text = "any,r1", Value = "any,r1" },
		new() { Text = "any,r2", Value = "any,r2" },
		new() { Text = "any PRG0", Value = "any PRG0" },
		new() { Text = "any PRG1", Value = "any PRG1" },
		new() { Text = "any PRG2", Value = "any PRG2" },
		new() { Text = "Europe", Value = "Europe" },
		new() { Text = "Europe v1.0", Value = "Europe v1.0" },
		new() { Text = "Europe v1.1", Value = "Europe v1.1" },
		new() { Text = "Europe,r0", Value = "Europe,r0" },
		new() { Text = "Europe,r1", Value = "Europe,r1" },
		new() { Text = "Europe,r2", Value = "Europe,r2" },
		new() { Text = "Europe PRG0", Value = "Europe PRG0" },
		new() { Text = "Europe PRG1", Value = "Europe PRG1" },
		new() { Text = "Europe PRG2", Value = "Europe PRG2" },
		new() { Text = "FDS", Value = "FDS" },
		new() { Text = "FDS v1.0", Value = "FDS v1.0" },
		new() { Text = "FDS v1.1", Value = "FDS v1.1" },
		new() { Text = "FDS,r0", Value = "FDS,r0" },
		new() { Text = "FDS,r1", Value = "FDS,r1" },
		new() { Text = "FDS,r2", Value = "FDS,r2" },
		new() { Text = "FDS PRG0", Value = "FDS PRG0" },
		new() { Text = "FDS PRG1", Value = "FDS PRG1" },
		new() { Text = "FDS PRG2", Value = "FDS PRG2" },
		new() { Text = "JPN", Value = "JPN" },
		new() { Text = "JPN v1.0", Value = "JPN v1.0" },
		new() { Text = "JPN v1.1", Value = "JPN v1.1" },
		new() { Text = "JPN,r0", Value = "JPN,r0" },
		new() { Text = "JPN,r1", Value = "JPN,r1" },
		new() { Text = "JPN,r2", Value = "JPN,r2" },
		new() { Text = "JPN PRG0", Value = "JPN PRG0" },
		new() { Text = "JPN PRG1", Value = "JPN PRG1" },
		new() { Text = "JPN PRG2", Value = "JPN PRG2" },
		new() { Text = "JPN/USA", Value = "JPN/USA" },
		new() { Text = "JPN/USA v1.0", Value = "JPN/USA v1.0" },
		new() { Text = "JPN/USA v1.1", Value = "JPN/USA v1.1" },
		new() { Text = "JPN/USA,r0", Value = "JPN/USA,r0" },
		new() { Text = "JPN/USA,r1", Value = "JPN/USA,r1" },
		new() { Text = "JPN/USA,r2", Value = "JPN/USA,r2" },
		new() { Text = "JPN/USA PRG0", Value = "JPN/USA PRG0" },
		new() { Text = "JPN/USA PRG1", Value = "JPN/USA PRG1" },
		new() { Text = "JPN/USA PRG2", Value = "JPN/USA PRG2" },
		new() { Text = "USA", Value = "USA" },
		new() { Text = "USA v1.0", Value = "USA v1.0" },
		new() { Text = "USA v1.1", Value = "USA v1.1" },
		new() { Text = "USA,r0", Value = "USA,r0" },
		new() { Text = "USA,r1", Value = "USA,r1" },
		new() { Text = "USA,r2", Value = "USA,r2" },
		new() { Text = "USA PRG0", Value = "USA PRG0" },
		new() { Text = "USA PRG1", Value = "USA PRG1" },
		new() { Text = "USA PRG2", Value = "USA PRG2" },
		new() { Text = "USA/Europe", Value = "USA/Europe" },
		new() { Text = "USA/Europe v1.0", Value = "USA/Europe v1.0" },
		new() { Text = "USA/Europe v1.1", Value = "USA/Europe v1.1" },
		new() { Text = "USA/Europe,r0", Value = "USA/Europe,r0" },
		new() { Text = "USA/Europe,r1", Value = "USA/Europe,r1" },
		new() { Text = "USA/Europe,r2", Value = "USA/Europe,r2" },
		new() { Text = "USA/Europe PRG0", Value = "USA/Europe PRG0" },
		new() { Text = "USA/Europe PRG1", Value = "USA/Europe PRG1" },
		new() { Text = "USA/Europe PRG2", Value = "USA/Europe PRG2" }
	};
}
