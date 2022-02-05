using AutoMapper;
using TASVideos.Api.Responses;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Pages.Games.Models;
using TASVideos.Pages.Games.Roms.Models;
using TASVideos.Pages.Publications.Models;
using TASVideos.Pages.RamAddresses.Models;
using TASVideos.Pages.Roles.Models;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Pages.UserFiles.Models;
using TASVideos.Pages.Users.Models;
using TASVideos.Pages.Wiki.Models;
using TASVideos.ViewComponents;

namespace TASVideos;

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
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code))
			.ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.GameGenres.Select(gg => gg.GenreId)));
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
							UserName = ur.User!.UserName
						})
						.ToList()));

		CreateMap<ForumPost, TopicFeedModel.TopicPost>()
			.ForMember(dest => dest.PosterName, opt => opt.MapFrom(src => src.Poster!.UserName));

		CreateMap<UserFile, UserMovieListModel>()
			.ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.UserName));

		CreateMap<UserFile, UserFileModel>()
			.ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.UserName))
			.ForMember(dest => dest.AuthorUserFilesCount, opt => opt.MapFrom(src => src.Author!.UserFiles.Count(uf => !uf.Hidden)))
			.ForMember(dest => dest.FileSizeUncompressed, opt => opt.MapFrom(src => src.LogicalLength))
			.ForMember(dest => dest.FileSizeCompressed, opt => opt.MapFrom(src => src.PhysicalLength))
			.ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.Game != null ? src.Game.Id : (int?)null))
			.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game != null ? src.Game.DisplayName : ""))
			.ForMember(dest => dest.GameSystem, opt => opt.MapFrom(src => src.Game != null ? src.Game.System!.Code : ""))
			.ForMember(dest => dest.System, opt => opt.MapFrom(src => src.System != null ? src.System.DisplayName : ""))
			.ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments
				.Select(c => new UserFileModel.UserFileCommentModel
				{
					Id = c.Id,
					Text = c.Text,
					CreationTimeStamp = c.CreationTimeStamp,
					UserId = c.UserId,
					UserName = c.User!.UserName
				})
				.ToList()));

		CreateMap<Publication, PublicationDisplayModel>()
			.ForMember(dest => dest.OnlineWatchingUrls, opt => opt.MapFrom(src => src.PublicationUrls.Where(pu => pu.Type == PublicationUrlType.Streaming && pu.Url != null).Select(pu => new PublicationDisplayModel.PublicationUrl(pu.Url!, pu.DisplayName))))
			.ForMember(dest => dest.MirrorSiteUrls, opt => opt.MapFrom(src => src.PublicationUrls.Where(pu => pu.Type == PublicationUrlType.Mirror && pu.Url != null).Select(pu => new PublicationDisplayModel.PublicationUrl(pu.Url!, pu.DisplayName))))
			.ForMember(dest => dest.RatingCount, opt => opt.MapFrom(src => src.PublicationRatings.Count))
			.ForMember(dest => dest.ClassIconPath, opt => opt.MapFrom(src => src.PublicationClass!.IconPath))
			.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game!.DisplayName))
			.ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Submission!.TopicId))
			.ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Files
				.Select(f => new PublicationDisplayModel.FileModel
				{
					Id = f.Id,
					Path = f.Path,
					Type = f.Type,
					Description = f.Description
				})
				.ToList()))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PublicationTags
				.Select(pt => new PublicationDisplayModel.TagModel
				{
					DisplayName = pt.Tag!.DisplayName,
					Code = pt.Tag.Code
				})
				.ToList()))
			.ForMember(dest => dest.GenreTags, opt => opt.MapFrom(src => src.Game!.GameGenres
				.Select(gg => new PublicationDisplayModel.TagModel
				{
					DisplayName = gg.Genre!.DisplayName,
					Code = gg.Genre.DisplayName // TODO
				})
				.ToList()))
			.ForMember(dest => dest.Flags, opt => opt.MapFrom(src => src.PublicationFlags
				.Select(pf => new PublicationDisplayModel.FlagModel
				{
					IconPath = pf.Flag!.IconPath,
					LinkPath = pf.Flag!.LinkPath,
					Name = pf.Flag.Name
				})
				.ToList()))
			.ForMember(dest => dest.ObsoletedMovies, opt => opt.MapFrom(src => src.ObsoletedMovies
				.Select(p => new PublicationDisplayModel.ObsoletesModel
				{
					Id = p.Id,
					Title = p.Title
				})
				.ToList()));

		CreateMap<UserFileComment, UserFileModel.UserFileCommentModel>()
			.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User!.UserName));

		CreateMap<Game, GameDisplayModel>()
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code))
			.ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.GameGenres.Select(gg => gg.Genre!.DisplayName)))
			.ForMember(dest => dest.Roms, opt => opt.MapFrom(src => src.Roms))
			.ForMember(dest => dest.GameGroups, opt => opt.MapFrom(src => src.GameGroups.Select(gg => gg.GameGroup)))
			.ForMember(dest => dest.PublicationCount, opt => opt.MapFrom(src => src.Publications.Count(p => p.ObsoletedById == null)))
			.ForMember(dest => dest.ObsoletePublicationCount, opt => opt.MapFrom(src => src.Publications.Count(p => p.ObsoletedById != null)))
			.ForMember(dest => dest.SubmissionCount, opt => opt.MapFrom(src => src.Submissions.Count))
			.ForMember(dest => dest.UserFilesCount, opt => opt.MapFrom(src => src.UserFiles.Count(uf => !uf.Hidden)));

		CreateMap<GameRom, GameDisplayModel.Rom>();
		CreateMap<GameGroup, GameDisplayModel.GameGroup>();

		CreateMap<PublicationFile, PublicationFileDisplayModel>();

		CreateMap<Submission, SubmissionPublishModel>()
			.ForMember(dest => dest.Markup, opt => opt.MapFrom(src => src.WikiContent!.Markup))
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code))
			.ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.SystemId ?? 0))
			.ForMember(dest => dest.SystemRegion, opt => opt.MapFrom(src => src.SystemFrameRate!.RegionCode + " " + src.SystemFrameRate.FrameRate))
			.ForMember(dest => dest.Game, opt => opt.MapFrom(src => src.Game!.DisplayName))
			.ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.GameId ?? 0))
			.ForMember(dest => dest.RomId, opt => opt.MapFrom(src => src.RomId ?? 0))
			.ForMember(dest => dest.Rom, opt => opt.MapFrom(src => src.Rom!.Name))
			.ForMember(dest => dest.PublicationClass, opt => opt.MapFrom(src => src.IntendedClass != null ? src.IntendedClass.Name : ""));

		// API
		CreateMap<Publication, PublicationsResponse>()
			.ForMember(dest => dest.Class, opt => opt.MapFrom(src => src.PublicationClass!.Name))
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code))
			.ForMember(dest => dest.SystemFrameRate, opt => opt.MapFrom(src => src.SystemFrameRate!.FrameRate))
			.ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors.OrderBy(pa => pa.Ordinal).Select(a => a.Author!.UserName).ToList()))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PublicationTags.Select(a => a.Tag!.Code).ToList()))
			.ForMember(dest => dest.Flags, opt => opt.MapFrom(src => src.PublicationFlags
				.Select(a => a.Flag!.Token)
				.ToList()))
			.ForMember(dest => dest.Urls, opt => opt.MapFrom(src => src.PublicationUrls
				.Select(u => u.Url)
				.ToList()))
			.ForMember(dest => dest.FilePaths, opt => opt.MapFrom(src => src.Files
				.Select(u => u.Path)
				.ToList()));

		CreateMap<Submission, SubmissionsResponse>()
			.ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.SubmissionAuthors.OrderBy(sa => sa.Ordinal).Select(a => a.Author!.UserName).ToList()))
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
			.ForMember(dest => dest.IntendedClass, opt => opt.MapFrom(src => src.IntendedClass != null ? src.IntendedClass.Name : null))
			.ForMember(dest => dest.Judge, opt => opt.MapFrom(src => src.Judge != null ? src.Judge.UserName : null))
			.ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher != null ? src.Publisher!.UserName : null))
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System != null ? src.System.Code : null))
			.ForMember(dest => dest.SystemFrameRate, opt => opt.MapFrom(src => src.SystemFrameRate != null ? src.SystemFrameRate.FrameRate : (double?)null));

		CreateMap<GameSystem, SystemsResponse>();
		CreateMap<GameSystemFrameRate, SystemsResponse.FrameRates>();

		CreateMap<Game, GamesResponse>();
		CreateMap<GameRom, GamesResponse.GameRom>();

		CreateMap<WikiPage, UserWikiEditHistoryModel.EditEntry>();
		CreateMap<GameRamAddress, AddressEditModel>()
			.ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game!.DisplayName))
			.ForMember(dest => dest.SystemCode, opt => opt.MapFrom(src => src.System!.Code));
	}
}
