using System.Linq;
using AutoMapper;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Pages.Game.Model;
using TASVideos.Pages.Game.Rom.Models;
using TASVideos.Pages.Publications.Models;
using TASVideos.Pages.Roles.Models;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Pages.UserFiles.Models;
using TASVideos.Pages.Users.Models;
using TASVideos.Pages.Wiki.Models;
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

			CreateMap<GameRom, RomEditModel>().ReverseMap();

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

			CreateMap<ForumPost, TopicFeedModel.TopicPost>()
				.ForMember(dest => dest.PosterName, opt => opt.MapFrom(src => src.Poster.UserName));

			// TODO: for both of these, the property could be renamed from Uploaded to UploadedTimestamp and reduce some complexity
			CreateMap<UserFile, UserMovieListModel>()
				.ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.UserName))
				.ForMember(dest => dest.Uploaded, opt => opt.MapFrom(src => src.UploadTimestamp));

			CreateMap<UserFile, UserFileModel>()
				.ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.UserName))
				.ForMember(dest => dest.Uploaded, opt => opt.MapFrom(src => src.UploadTimestamp))
				.ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.LogicalLength))
				.ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.Game != null ? src.Game.Id : (int?)null))
				.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game != null ? src.Game.DisplayName : ""))
				.ForMember(dest => dest.System, opt => opt.MapFrom(src => src.System != null ? src.System.DisplayName : ""));
		}
	}
}
