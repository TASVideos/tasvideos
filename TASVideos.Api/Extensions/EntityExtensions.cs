using TASVideos.Api.Responses;
using TASVideos.Data.Entity;
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

	public static IQueryable<PublicationsResponse> ToPublicationsResponse(this IQueryable<Publication> query)
	{
		return query.Select(p => new PublicationsResponse
		{
			Id = p.Id,
			Title = p.Title,
			Branch = p.Branch,
			EmulatorVersion = p.EmulatorVersion,
			Class = p.PublicationClass!.Name,
			SystemCode = p.System!.Code,
			SubmissionId = p.SubmissionId,
			GameId = p.GameId,
			GameVersionId = p.GameVersionId,
			ObsoletedById = p.ObsoletedById,
			Frames = p.Frames,
			RerecordCount = p.RerecordCount,
			SystemFrameRate = p.SystemFrameRate!.FrameRate,
			MovieFileName = p.MovieFileName,
			Authors = p.Authors
				.OrderBy(pa => pa.Ordinal)
				.Select(a => a.Author!.UserName),
			Tags = p.PublicationTags
				.Select(a => a.Tag!.Code),
			Flags = p.PublicationFlags
				.Select(pf => pf.Flag!.Token),
			Urls = p.PublicationUrls
				.Select(pu => pu.Url!),
			FilePaths = p.Files
				.Select(f => f.Path)
		});
	}
}
