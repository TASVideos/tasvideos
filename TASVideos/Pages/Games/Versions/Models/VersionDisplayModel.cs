using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions.Models;

public record VersionDisplayModel(
	string SystemCode,
	string Name,
	string? Md5,
	string? Sha1,
	string? Version,
	string? Region,
	VersionTypes Type,
	string? TitleOverride,
	int GameId,
	string GameName);
