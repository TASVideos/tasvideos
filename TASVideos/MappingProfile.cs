using System.Linq;
using AutoMapper;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Pages.Roles.Models;
using TASVideos.Tasks;
using TASVideos.ViewComponents;

namespace TASVideos
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<SubmissionCatalogModel, Submission>();
			CreateMap<PublicationCatalogModel, Publication>();

			CreateMap<User, UserEditModel>()
				.ForMember(dest => dest.IsLockedOut, opt => opt.MapFrom(src => src.LockoutEnabled && src.LockoutEnd.HasValue))
				.ForMember(dest => dest.SelectedRoles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.RoleId).ToList()));

			CreateMap<WikiPage, UserWikiEditHistoryModel>();

			CreateMap<Game, GameEditModel>()
				.ForMember(
					dest => dest.SystemCode, 
					opt => opt
						.MapFrom(src => 
							src.System.Code));
			CreateMap<GameEditModel, Game>();

			CreateMap<GameRom, RomEditModel>()
				.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.DisplayName));
			CreateMap<RomEditModel, GameRom>();

			CreateMap<AwardTasks.AwardDto, AwardDetailsModel>();
			CreateMap<AwardTasks.AwardDto.UserDto, AwardDetailsModel.UserModel>();

			CreateMap<Role, RoleDisplayModel>()
				.ForMember(
					dest => dest.Permissions,
					opt => opt.MapFrom(src =>
						src.RolePermission
							.Select(rp => rp.PermissionId)
							.ToList()))
				.ForMember(
					dest => dest.Links, 
					opt => opt.MapFrom(src => 
						src.RoleLinks
							.Select(rl => rl.Link)
							.ToList()))
				.ForMember(
					dest => dest.Users,
					opt => opt.MapFrom(src =>
						src.UserRole
							.Select(ur => new RoleDisplayModel.UserWithRole
							{
								Id = ur.UserId,
								UserName = ur.User.UserName
							})
							.ToList()));

			CreateMap<ForumPost, TopicFeedModel.TopicPost>();
		}
	}
}
