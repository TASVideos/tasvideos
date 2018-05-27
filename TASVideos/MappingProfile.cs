using AutoMapper;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos
{
	// https://stackoverflow.com/questions/40275195/how-to-setup-automapper-in-asp-net-core
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<SubmissionCreateViewModel, Submission>()
				.ForMember(dest => dest.EmulatorVersion, opt => opt.MapFrom(src => src.Emulator));

			CreateMap<SubmissionCatalogModel, Submission>();
			CreateMap<PublicationCatalogModel, Publication>();

			CreateMap<User, UserEditViewModel>()
				.ForMember(dest => dest.IsLockedOut, opt => opt.MapFrom(src => src.LockoutEnabled && src.LockoutEnd.HasValue));

			CreateMap<WikiPage, UserWikiEditHistoryModel>();

			CreateMap<Game, GameEditModel>()
				.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System.Code));
			CreateMap<GameEditModel, Game>();

			CreateMap<GameRom, RomEditModel>()
				.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.DisplayName));
			CreateMap<RomEditModel, GameRom>();

			CreateMap<AwardTasks.AwardDto, AwardDetailsModel>();
			CreateMap<AwardTasks.AwardDto.UserDto, AwardDetailsModel.UserModel>();
		}
	}
}
