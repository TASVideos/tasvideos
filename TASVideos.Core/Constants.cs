namespace TASVideos.Core;

public static class PostGroups
{
	public const string Forum = "Forum";
	public const string Wiki = "Wiki";
	public const string Submission = "Submission";
	public const string UserManagement = "UserManagement";
	public const string UserFiles = "UserFiles";
	public const string Publication = "Publication";
	public const string Game = "Game";
}

internal static class HttpClients
{
	public const string GoogleAuth = "GoogleAuth";
	public const string Youtube = "Youtube";
	public const string Discord = "Discord";
	public const string Bluesky = "Bluesky";
}

internal static class PlayerPointConstants
{
	public const double ObsoleteMultiplier = 0.000001;
	public const int MinimumPlayerPointsForPublication = 5;
}

internal static class PlayerRanks
{
	public const string FormerPlayer = "Former player";
	public const string Player = "Player";
	public const string ActivePlayer = "Active player";
	public const string ExperiencedPlayer = "Experienced player";
	public const string SkilledPlayer = "Skilled player";
	public const string ExpertPlayer = "Expert player";
}
