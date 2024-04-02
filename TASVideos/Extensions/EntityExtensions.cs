using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Pages.Forum.Posts;
using TASVideos.Pages.Publications.Models;
using TASVideos.Pages.Roles;
using TASVideos.Pages.Submissions.Models;
using TASVideos.WikiModules;

namespace TASVideos.Extensions;

/// <summary>
/// Web front-end specific extension methods for Entity Framework POCOs
/// </summary>
public static class EntityExtensions
{
	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<string> query)
	{
		return query.Select(s => new SelectListItem
		{
			Text = s,
			Value = s
		});
	}

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<string> strings)
	{
		return strings.Select(s => new SelectListItem
		{
			Text = s,
			Value = s
		});
	}

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<PermissionTo> permissions)
	{
		return permissions.Select(p => new SelectListItem
		{
			Text = p.ToString().SplitCamelCase(),
			Value = ((int)p).ToString()
		});
	}

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<int> ints)
	{
		return ints.Select(i => new SelectListItem
		{
			Text = i.ToString(),
			Value = i.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<Genre> query)
	{
		return query.Select(g => new SelectListItem
		{
			Text = g.DisplayName,
			Value = g.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<GameGroup> query)
	{
		return query.Select(g => new SelectListItem
		{
			Text = g.Name,
			Value = g.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<GameSystem> query)
	{
		return query.Select(s => new SelectListItem
		{
			Text = s.Code,
			Value = s.Code
		});
	}

	public static IQueryable<SelectListItem> ToDropDownWithId(this IQueryable<GameSystem> query)
	{
		return query.Select(s => new SelectListItem
		{
			Text = s.Id.ToString(),
			Value = s.Code
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<PublicationClass> query)
	{
		return query.Select(p => new SelectListItem
		{
			Text = p.Name,
			Value = p.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<SubmissionRejectionReason> query)
	{
		return query.Select(r => new SelectListItem
		{
			Text = r.DisplayName,
			Value = r.Id.ToString()
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
				Text = g.RegionCode + " " + g.FrameRate + (g.Obsolete ? " (Obsolete)" : ""),
				Value = g.Id.ToString()
			});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<Flag> query, IEnumerable<PermissionTo> userPermissions)
	{
		return query
			.OrderBy(f => f.Name)
			.Select(f => new SelectListItem
			{
				Text = f.Name,
				Value = f.Id.ToString(),
				Disabled = f.PermissionRestriction.HasValue
					&& !userPermissions.Contains(f.PermissionRestriction.Value)
			});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Tag> query)
	{
		return query
			.OrderBy(t => t.DisplayName)
			.Select(t => new SelectListItem
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

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Forum> query, int forumId)
	{
		return query.Select(f => new SelectListItem
		{
			Text = f.Name,
			Value = f.Id.ToString(),
			Selected = f.Id == forumId
		});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<ForumTopic> query)
	{
		return query.Select(t => new SelectListItem
		{
			Text = t.Title,
			Value = t.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<User> query)
	{
		return query.Select(u => new SelectListItem
		{
			Text = u.UserName,
			Value = u.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Award> query, int year)
	{
		return query.Select(a => new SelectListItem
		{
			Text = a.Description + " for " + year,
			Value = a.ShortName
		});
	}

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<AssignableRole> roles)
	{
		return roles.Select(r => new SelectListItem
		{
			Text = r.Name,
			Value = r.Id.ToString(),
			Disabled = r.Disabled
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<GameVersion> query)
	{
		return query.Select(v => new SelectListItem
		{
			Text = v.Name,
			Value = v.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<Game> query)
	{
		return query.Select(g => new SelectListItem
		{
			Text = g.DisplayName,
			Value = g.Id.ToString()
		});
	}

	public static IQueryable<SelectListItem> ToDropDown(this IQueryable<GameGoal> query)
	{
		return query.Select(gg => new SelectListItem
		{
			Text = gg.DisplayName,
			Value = gg.Id.ToString()
		});
	}

	public static IEnumerable<SelectListItem> ToDopDown(this IEnumerable<Tag> tags)
	{
		return tags.Select(t => new SelectListItem
		{
			Text = t.DisplayName,
			Value = t.Code.ToLower()
		});
	}

	public static IEnumerable<SelectListItem> ToDopDown(this IEnumerable<Flag> flags)
	{
		return flags.Select(f => new SelectListItem
		{
			Text = f.Token.ToLower(),
			Value = f.Name
		});
	}

	public static List<SelectListItem> ToDropDown<T>(this IEnumerable<T> enums)
		where T : Enum
	{
		return enums
			.Select(e => new SelectListItem
			{
				Value = ((int)(object)e).ToString(),
				Text = e.EnumDisplayName()
			})
			.ToList();
	}

	public static List<SelectListItem> WithDefaultEntry(this IEnumerable<SelectListItem> items)
	{
		return [.. UiDefaults.DefaultEntry, .. items];
	}

	public static List<SelectListItem> WithAnyEntry(this IEnumerable<SelectListItem> items)
	{
		return [.. UiDefaults.AnyEntry, .. items];
	}

	public static IQueryable<SubmissionListEntry> ToSubListEntry(this IQueryable<Submission> query)
	{
		return query
			.Select(s => new SubmissionListEntry
			{
				Id = s.Id,
				System = s.System != null ? s.System!.Code : "Unknown",
				GameName = s.GameVersion != null && !string.IsNullOrWhiteSpace(s.GameVersion.TitleOverride) ? s.GameVersion.TitleOverride : s.Game != null ? s.Game.DisplayName : s.GameName,
				Frames = s.Frames,
				FrameRate = s.SystemFrameRateId != null ? s.SystemFrameRate!.FrameRate : 60,
				Branch = s.Branch,
				Authors = s.SubmissionAuthors.OrderBy(sa => sa.Ordinal).Select(sa => sa.Author!.UserName).ToList(),
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
				GameVersionId = p.GameVersionId,
				GameVersionName = p.GameVersion!.Name,
				CreateTimestamp = p.CreateTimestamp,
				LastUpdateTimestamp = p.LastUpdateTimestamp,
				ObsoletedById = p.ObsoletedById,
				Title = p.Title,
				Class = p.PublicationClass!.Name,
				ClassIconPath = p.PublicationClass!.IconPath,
				MovieFileName = p.MovieFileName,
				SubmissionId = p.SubmissionId,
				Urls = p.PublicationUrls
					.Select(pu => new PublicationDisplayModel.PublicationUrl(pu.Type, pu.Url!, pu.DisplayName))
					.ToList(),
				TopicId = p.Submission!.TopicId ?? 0,
				EmulatorVersion = p.EmulatorVersion,
				Tags = p.PublicationTags
					.Select(pt => new PublicationDisplayModel.TagModel(
						pt.Tag!.DisplayName,
						pt.Tag.Code)),
				GameGenres = p.Game!.GameGenres.Select(gg => gg.Genre!.DisplayName),
				Files = p.Files
					.Select(f => new PublicationDisplayModel.FileModel(
						f.Id,
						f.Path,
						f.Type,
						f.Description)),
				Flags = p.PublicationFlags
					.Select(pf => new PublicationDisplayModel.FlagModel(
						pf.Flag!.IconPath,
						pf.Flag!.LinkPath,
						pf.Flag.Name)),
				ObsoletedMovies = p.ObsoletedMovies
					.Select(o => new PublicationDisplayModel.ObsoletesModel(o.Id, o.Title)),
				RatingCount = p.PublicationRatings.Count,
				OverallRating = p.PublicationRatings
					.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
					.Where(pr => pr.User!.UseRatings)
					.Average(pr => pr.Value),
				Rating = new PublicationRateModel
				{
					Rating = p.PublicationRatings.Where(pr => pr.UserId == userId).Select(pr => pr.Value.ToString()).FirstOrDefault(),
					Unrated = p.PublicationRatings.All(pr => pr.UserId != userId)
				},
			});

		if (ratingSort)
		{
			q = q
				.OrderByDescending(p => p.OverallRating ?? 0)
				.ThenByDescending(p => p.RatingCount);
		}

		return q;
	}

	public static IQueryable<ListModel.RoleDisplay> ToRoleDisplayModel(this IQueryable<Role> roles)
	{
		return roles.Select(r => new ListModel.RoleDisplay
		{
			IsDefault = r.IsDefault,
			Id = r.Id,
			Name = r.Name,
			Description = r.Description,
			Permissions = r.RolePermission.Select(rp => rp.PermissionId).ToList(),
			Links = r.RoleLinks.Select(rl => rl.Link).ToList(),
			Users = r.UserRole
				.Select(ur => new ListModel.RoleDisplay.UserWithRole(ur.UserId, ur.User!.UserName))
				.ToList()
		});
	}

	public static IQueryable<UserFileModel> ToUserFileModel(this IQueryable<UserFile> userFiles, bool hideComments = true)
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
				.Select(c => new UserFileModel.UserFileCommentModel(c.Id, c.Text, c.CreationTimeStamp, c.UserId, c.User!.UserName))
				.ToList(),
			HideComments = hideComments,
			Annotations = uf.Annotations
		});
	}

	public static IQueryable<Pages.UserFiles.IndexModel.UserMovie> ToUserMovieListModel(this IQueryable<UserFile> userFiles)
	{
		return userFiles.Select(uf => new Pages.UserFiles.IndexModel.UserMovie(
			uf.Id,
			uf.Author!.UserName,
			uf.UploadTimestamp,
			uf.FileName,
			uf.Title));
	}

	public static IQueryable<DisplayMiniMovie.MiniMovieModel> ToMiniMovieModel(this IQueryable<Publication> publications)
	{
		return publications.Select(p => new DisplayMiniMovie.MiniMovieModel
		{
			Id = p.Id,
			Title = p.Title,
			Goal = p.GameGoal!.DisplayName,
			Screenshot = p.Files
				.Where(f => f.Type == FileType.Screenshot)
				.Select(f => new DisplayMiniMovie.MiniMovieModel.ScreenshotFile
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
			Branch = s.Branch,
			GameGoalId = s.GameGoalId
		});
	}

	public static IQueryable<AddEditModel.RoleEditModel> ToRoleEditModel(this IQueryable<Role> query)
	{
		return query.Select(r => new AddEditModel.RoleEditModel
		{
			Name = r.Name,
			IsDefault = r.IsDefault,
			Description = r.Description,
			AutoAssignPostCount = r.AutoAssignPostCount,
			AutoAssignPublications = r.AutoAssignPublications,
			Links = r.RoleLinks
				.Select(rl => rl.Link)
				.ToList(),
			SelectedPermissions = r.RolePermission
				.Select(rp => (int)rp.PermissionId)
				.ToList(),
			SelectedAssignablePermissions = r.RolePermission
				.Where(rp => rp.CanAssign)
				.Select(rp => (int)rp.PermissionId)
				.ToList()
		});
	}

	public static IQueryable<Pages.UserFiles.Uncataloged.UncatalogedViewModel> ToUnCatalogedModel(this IQueryable<UserFile> query)
	{
		return query.Select(uf => new Pages.UserFiles.Uncataloged.UncatalogedViewModel(
			uf.Id,
			uf.FileName,
			uf.System != null ? uf.System.Code : null,
			uf.UploadTimestamp,
			uf.Author!.UserName));
	}

	public static IQueryable<LatestModel.LatestPost> ToLatestPost(this IQueryable<ForumPost> query)
	{
		return query.Select(p => new LatestModel.LatestPost
		{
			Id = p.Id,
			CreateTimestamp = p.CreateTimestamp,
			Text = p.Text,
			TopicId = p.TopicId ?? 0,
			TopicTitle = p.Topic!.Title,
			ForumId = p.Topic.ForumId,
			ForumName = p.Topic!.Forum!.Name,
			PosterName = p.Poster!.UserName
		});
	}
}
