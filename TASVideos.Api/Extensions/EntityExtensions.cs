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

	public static IQueryable<SystemsResponse> ToSystemsResponse(this IQueryable<GameSystem> query)
	{
		return query.Select(q => new SystemsResponse
		{
			Id = q.Id,
			Code = q.Code,
			DisplayName = q.DisplayName,
			SystemFrameRates = q.SystemFrameRates.Select(sf => new SystemsResponse.FrameRates
			{
				FrameRate = sf.FrameRate,
				RegionCode = sf.RegionCode,
				Preliminary = sf.Preliminary,
				Obsolete = sf.Obsolete
			})
		});
	}
}
