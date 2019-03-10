using System.Linq;
using AutoMapper;

using TASVideos.Api.Responses;
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

			CreateMap<UserFile, UserMovieListModel>()
				.ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.UserName));

			CreateMap<UserFile, UserFileModel>()
				.ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.UserName))
				.ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.LogicalLength))
				.ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.Game != null ? src.Game.Id : (int?)null))
				.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game != null ? src.Game.DisplayName : ""))
				.ForMember(dest => dest.System, opt => opt.MapFrom(src => src.System != null ? src.System.DisplayName : ""));


			// API
			CreateMap<Publication, PublicationsResponse>()
				.ForMember(dest => dest.Tier, opt => opt.MapFrom(src => src.Tier.Name))
				.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System.Code))
				.ForMember(dest => dest.SystemFrameRate, opt => opt.MapFrom(src => src.SystemFrameRate.FrameRate))
				.ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors.Select(a => a.Author.UserName).ToList()))
				.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PublicationTags.Select(a => a.Tag.Code).ToList()))
				.ForMember(dest => dest.Flags, opt => opt.MapFrom(src => src.PublicationFlags.Select(a => a.Flag.Token).ToList()));
		}
	}
}
