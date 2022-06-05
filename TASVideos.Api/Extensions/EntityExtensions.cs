using TASVideos.Api.Responses;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api;

internal static class EntityExtensions
{
	public static IQueryable<GamesResponse> ToGamesResponse(this IQueryable<Game> query)
	{
		return query.Select(q => new GamesResponse
		{
			Id = q.Id,
			GoodName = q.GoodName,
			DisplayName = q.DisplayName,
			Abbreviation = q.Abbreviation,
			SearchKey = q.SearchKey,
			YoutubeTags = q.YoutubeTags,
			ScreenshotUrl = q.ScreenshotUrl,
			Versions = q.GameVersions.Select(gv => new GamesResponse.GameVersion
			{
				Id = gv.Id,
				Md5 = gv.Md5,
				Sha1 = gv.Sha1,
				Name = gv.Name,
				Type = gv.Type,
				Region = gv.Region,
				Version = gv.Version,
				SystemCode = gv.System!.Code
			})
		});
	}
}
