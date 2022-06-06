using AutoMapper;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Models;
using TASVideos.Pages.Games.Versions.Models;
using TASVideos.Pages.Publications.Models;
using TASVideos.Pages.RamAddresses.Models;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Pages.Wiki.Models;
using TASVideos.ViewComponents;

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

		CreateMap<ForumPost, TopicFeedModel.TopicPost>()
			.ForMember(dest => dest.PosterName, opt => opt.MapFrom(src => src.Poster!.UserName));

		CreateMap<Game, GameDisplayModel>()
			.ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.GameGenres.Select(gg => gg.Genre!.DisplayName)))
			.ForMember(dest => dest.Versions, opt => opt.MapFrom(src => src.GameVersions))
			.ForMember(dest => dest.GameGroups, opt => opt.MapFrom(src => src.GameGroups.Select(gg => gg.GameGroup)))
			.ForMember(dest => dest.PublicationCount, opt => opt.MapFrom(src => src.Publications.Count(p => p.ObsoletedById == null)))
			.ForMember(dest => dest.ObsoletePublicationCount, opt => opt.MapFrom(src => src.Publications.Count(p => p.ObsoletedById != null)))
			.ForMember(dest => dest.SubmissionCount, opt => opt.MapFrom(src => src.Submissions.Count))
			.ForMember(dest => dest.UserFilesCount, opt => opt.MapFrom(src => src.UserFiles.Count(uf => !uf.Hidden)));

		CreateMap<GameVersion, GameDisplayModel.GameVersion>()
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code));
		CreateMap<GameGroup, GameDisplayModel.GameGroup>();

		CreateMap<PublicationFile, PublicationFileDisplayModel>();

		CreateMap<Submission, SubmissionPublishModel>()
			.ForMember(dest => dest.Markup, opt => opt.MapFrom(src => src.WikiContent!.Markup))
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code))
			.ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.SystemId ?? 0))
			.ForMember(dest => dest.SystemRegion, opt => opt.MapFrom(src => src.SystemFrameRate!.RegionCode + " " + src.SystemFrameRate.FrameRate))
			.ForMember(dest => dest.Game, opt => opt.MapFrom(src => src.Game!.DisplayName))
			.ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.GameId ?? 0))
			.ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.GameVersionId ?? 0))
			.ForMember(dest => dest.GameVersion, opt => opt.MapFrom(src => src.GameVersion!.Name))
			.ForMember(dest => dest.PublicationClass, opt => opt.MapFrom(src => src.IntendedClass != null ? src.IntendedClass.Name : ""));

		CreateMap<WikiPage, UserWikiEditHistoryModel.EditEntry>();
		CreateMap<GameRamAddress, AddressEditModel>()
			.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game!.DisplayName))
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code));
	}
}
