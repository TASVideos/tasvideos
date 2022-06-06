using AutoMapper;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Models;
using TASVideos.Pages.Games.Versions.Models;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<Game, GameEditModel>()
			.ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.GameGenres.Select(gg => gg.GenreId)))
			.ForMember(dest => dest.Groups, opt => opt.MapFrom(src => src.GameGroups.Select(ggr => ggr.GameGroupId)));
		CreateMap<GameEditModel, Game>();

		CreateMap<GameVersion, VersionEditModel>()
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code))
			.ReverseMap();

		CreateMap<WikiPage, UserWikiEditHistoryModel.EditEntry>();
	}
}
