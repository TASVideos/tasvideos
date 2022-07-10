using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Pages.Games.Models;
using TASVideos.Pages.Publications.Models;
using TASVideos.Pages.Roles.Models;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Pages.UserFiles.Models;
using TASVideos.Pages.Users.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Extensions;

/// <summary>
/// Web front-end specific extension methods for Entity Framework POCOs
/// </summary>
public static class EntityExtensions
{
	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<string> query)
	{
		return query
			.Select(s => new SelectListItem
			{
				Text = s,
				Value = s
			});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Genre> query)
	{
		return query
			.Select(s => new SelectListItem
			{
				Text = s.DisplayName,
				Value = s.Id.ToString()
			});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<GameGroup> query)
	{
		return query
			.Select(s => new SelectListItem
			{
				Text = s.Name,
				Value = s.Id.ToString()
			});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<GameSystem> query)
	{
		return query
			.Select(s => new SelectListItem
			{
				Text = s.Code,
				Value = s.Code
			});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<PublicationClass> query)
	{
		return query
			.Select(s => new SelectListItem
			{
				Text = s.Name,
				Value = s.Id.ToString()
			});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<SubmissionRejectionReason> query)
	{
		return query
			.Select(s => new SelectListItem
			{
				Text = s.DisplayName,
				Value = s.Id.ToString()
			});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<Game> query)
	{
		return query
			.Select(g => new SelectListItem
			{
				Text = g.DisplayName,
				Value = g.Id.ToString()
			});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<GameSystemFrameRate> query)
	{
		return query
			.OrderBy(fr => fr.Obsolete)
			.ThenBy(fr => fr.RegionCode)
			.ThenBy(fr => fr.FrameRate)
			.Select(g => new SelectListItem
			{
				Value = g.Id.ToString(),
				Text = g.RegionCode + " " + g.FrameRate + (g.Obsolete ? " (Obsolete)" : "")
			});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<Flag> query, IEnumerable<PermissionTo> userPermissions)
	{
		return query.Select(f => new SelectListItem
		{
			Text = f.Name,
			Value = f.Id.ToString(),
			Disabled = f.PermissionRestriction.HasValue
				&& !userPermissions.Contains(f.PermissionRestriction.Value)
		});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Tag> query)
	{
		return query.Select(t => new SelectListItem
		{
			Text = t.DisplayName,
			Value = t.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Publication> query)
	{
		return query.Select(p => new SelectListItem
		{
			Text = p.Title,
			Value = p.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<ForumCategory> query)
	{
		return query.Select(c => new SelectListItem
			{
				Text = c.Title,
				Value = c.Id.ToString()
			});
	}

	public static IQueryable<SubmissionListEntry> ToSubListEntry(this IQueryable<Submission> query)
	{
		return query
			.Select(s => new SubmissionListEntry
			{
				Id = s.Id,
				System = s.System!.Code,
				GameName = s.GameName,
				Frames = s.Frames,
				FrameRate = s.SystemFrameRate!.FrameRate,
				Branch = s.Branch,
				Authors = s.SubmissionAuthors.OrderBy(sa => sa.Ordinal).Select(sa => sa.Author!.UserName),
				AdditionalAuthors = s.AdditionalAuthors,
				Submitted = s.CreateTimestamp,
				Status = s.Status,
				Judge = s.Judge != null ? s.Judge.UserName : null,
				Publisher = s.Publisher != null ? s.Publisher.UserName : null,
				IntendedClass = s.IntendedClass != null ? s.IntendedClass.Name : null
			});
	}

	public static IQueryable<PublicationDisplayModel> ToViewModel(this IQueryable<Publication> query, bool ratingSort = false, int userId = -1)
	{
		var q = query
			.Select(p => new PublicationDisplayModel
			{
				Id = p.Id,
				GameId = p.GameId,
				GameName = p.Game!.DisplayName,
				CreateTimestamp = p.CreateTimestamp,
				LastUpdateTimestamp = p.LastUpdateTimestamp,
				ObsoletedById = p.ObsoletedById,
				Title = p.Title,
				ClassIconPath = p.PublicationClass!.IconPath,
				MovieFileName = p.MovieFileName,
				SubmissionId = p.SubmissionId,
				Urls = p.PublicationUrls
					.Select(pu => new PublicationDisplayModel.PublicationUrl(pu.Type, pu.Url!, pu.DisplayName))
					.ToList(),
				TopicId = p.Submission!.TopicId ?? 0,
				EmulatorVersion = p.EmulatorVersion,
				Tags = p.PublicationTags
					.Select(pt => new PublicationDisplayModel.TagModel
					{
						DisplayName = pt.Tag!.DisplayName,
						Code = pt.Tag.Code
					}),
				GameGenres = p.Game!.GameGenres.Select(gg => gg.Genre!.DisplayName),
				Files = p.Files
					.Select(f => new PublicationDisplayModel.FileModel
					{
						Id = f.Id,
						Path = f.Path,
						Type = f.Type,
						Description = f.Description
					}),
				Flags = p.PublicationFlags
					.Select(pf => new PublicationDisplayModel.FlagModel
					{
						IconPath = pf.Flag!.IconPath,
						LinkPath = pf.Flag!.LinkPath,
						Name = pf.Flag.Name
					}),
				ObsoletedMovies = p.ObsoletedMovies
					.Select(o => new PublicationDisplayModel.ObsoletesModel
					{
						Id = o.Id,
						Title = o.Title
					}),
				RatingCount = p.PublicationRatings.Count,
				OverallRating = p.PublicationRatings
					.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
					.Where(pr => pr.User!.UseRatings)
					.Average(pr => pr.Value),
				Rating = new PublicationRateModel
				{
					Rating = p.PublicationRatings.Where(pr => pr.UserId == userId).Select(pr => pr.Value.ToString()).FirstOrDefault(),
					Unrated = !p.PublicationRatings.Any(pr => pr.UserId == userId)
				},
				Region = p.GameVersion != null ? p.GameVersion.Region : null,
				GameVersion = p.GameVersion != null ? p.GameVersion.Version : null
			});

		if (ratingSort)
		{
			q = q.OrderByDescending(p => p.OverallRating);
		}

		return q;
	}

	public static IQueryable<UserEditModel> ToUserEditModel(this IQueryable<User> query)
	{
		return query.Select(u => new UserEditModel
		{
			UserName = u.UserName,
			TimezoneId = u.TimeZoneId,
			From = u.From,
			SelectedRoles = u.UserRoles.Select(ur => ur.RoleId),
			CreateTimestamp = u.CreateTimestamp,
			LastLoggedInTimeStamp = u.LastLoggedInTimeStamp,
			Email = u.Email,
			EmailConfirmed = u.EmailConfirmed,
			IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue,
			Signature = u.Signature,
			Avatar = u.Avatar,
			MoodAvatarUrlBase = u.MoodAvatarUrlBase,
			UseRatings = u.UseRatings,
			ModeratorComments = u.ModeratorComments
		});
	}

	public static IQueryable<RoleDisplayModel> ToRoleDisplayModel(this IQueryable<Role> roles)
	{
		return roles.Select(r => new RoleDisplayModel
		{
			IsDefault = r.IsDefault,
			Id = r.Id,
			Name = r.Name,
			Description = r.Description,
			Permissions = r.RolePermission.Select(rp => rp.PermissionId),
			Links = r.RoleLinks.Select(rl => rl.Link),
			Users = r.UserRole.Select(ur => new RoleDisplayModel.UserWithRole
			{
				Id = ur.UserId,
				UserName = ur.User!.UserName
			}).ToList()
		});
	}

	public static IQueryable<UserFileModel> ToUserFileModel(this IQueryable<UserFile> userFiles)
	{
		return userFiles.Select(uf => new UserFileModel
		{
			Id = uf.Id,
			Class = uf.Class,
			Title = uf.Title,
			Description = uf.Description,
			UploadTimestamp = uf.UploadTimestamp,
			Author = uf.Author!.UserName,
			AuthorUserFilesCount = uf.Author!.UserFiles.Count(auf => !auf.Hidden),
			Views = uf.Views,
			Downloads = uf.Downloads,
			Hidden = uf.Hidden,
			FileName = uf.FileName,
			FileSizeUncompressed = uf.LogicalLength,
			FileSizeCompressed = uf.PhysicalLength,
			GameId = uf.GameId,
			GameName = uf.Game != null
				? uf.Game.DisplayName
				: "",
			GameSystem = uf.System != null
				? uf.System.Code
				: "",
			System = uf.System != null
				? uf.System.DisplayName
				: "",
			Length = uf.Length,
			Frames = uf.Frames,
			Rerecords = uf.Rerecords,
			Comments = uf.Comments
				.Select(c => new UserFileModel.UserFileCommentModel
				{
					Id = c.Id,
					Text = c.Text,
					CreationTimeStamp = c.CreationTimeStamp,
					UserId = c.UserId,
					UserName = c.User!.UserName
				})
		});
	}

	public static IQueryable<UserMovieListModel> ToUserMovieListModel(this IQueryable<UserFile> userFiles)
	{
		return userFiles.Select(uf => new UserMovieListModel
		{
			Id = uf.Id,
			Author = uf.Author!.UserName,
			UploadTimestamp = uf.UploadTimestamp,
			FileName = uf.FileName,
			Title = uf.Title
		});
	}

	public static IQueryable<MiniMovieModel> ToMiniMovieModel(this IQueryable<Publication> publications)
	{
		return publications.Select(p => new MiniMovieModel
		{
			Id = p.Id,
			Title = p.Title,
			Branch = p.Branch ?? "",
			Screenshot = p.Files
				.Where(f => f.Type == FileType.Screenshot)
				.Select(f => new MiniMovieModel.ScreenshotFile
				{
					Path = f.Path,
					Description = f.Description
				})
				.First(),
			OnlineWatchingUrl = p.PublicationUrls
				.First(u => u.Type == PublicationUrlType.Streaming).Url
		});
	}

	public static IQueryable<SubmissionPublishModel> ToPublishModel(this IQueryable<Submission> submissions)
	{
		return submissions.Select(s => new SubmissionPublishModel
		{
			Markup = s.WikiContent!.Markup,
			SystemCode = s.System!.Code,
			SystemRegion = s.SystemFrameRate!.RegionCode + " " + s.SystemFrameRate.FrameRate,
			Game = s.Game!.DisplayName,
			GameId = s.GameId ?? 0,
			GameVersion = s.GameVersion!.Name,
			VersionId = s.GameVersionId ?? 0,
			PublicationClass = s.IntendedClass != null
				? s.IntendedClass.Name
				: "",
			MovieExtension = s.MovieExtension,
			Title = s.Title,
			SystemId = s.SystemId ?? 0,
			SystemFrameRateId = s.SystemFrameRateId,
			Status = s.Status,
			EmulatorVersion = s.EmulatorVersion,
			Branch = s.Branch
		});
	}

	public static IQueryable<GameDisplayModel> ToGameDisplayModel(this IQueryable<Game> games)
	{
		return games.Select(g => new GameDisplayModel
		{
			Id = g.Id,
			DisplayName = g.DisplayName,
			Abbreviation = g.Abbreviation,
			ScreenshotUrl = g.ScreenshotUrl,
			GameResourcesPage = g.GameResourcesPage,
			Genres = g.GameGenres.Select(gg => gg.Genre!.DisplayName),
			Versions = g.GameVersions.Select(gv => new GameDisplayModel.GameVersion
			{
				Type = gv.Type,
				Id = gv.Id,
				Md5 = gv.Md5,
				Sha1 = gv.Sha1,
				Name = gv.Name,
				Region = gv.Region,
				Version = gv.Version,
				SystemCode = gv.System!.Code,
				TitleOverride = gv.TitleOverride
			}).ToList(),
			GameGroups = g.GameGroups.Select(gg => new GameDisplayModel.GameGroup
			{
				Id = gg.GameGroupId,
				Name = gg.GameGroup!.Name
			}).ToList(),
			PublicationCount = g.Publications.Count(p => p.ObsoletedById == null),
			ObsoletePublicationCount = g.Publications.Count(p => p.ObsoletedById != null),
			SubmissionCount = g.Submissions.Count,
			UserFilesCount = g.UserFiles.Count(uf => !uf.Hidden)
		});
	}
}
