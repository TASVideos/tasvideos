using TASVideos.Common;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Forum.Posts;
using TASVideos.Pages.Roles;
using TASVideos.WikiModules;

namespace TASVideos.Extensions;

/// <summary>
/// Web front-end-specific extension methods for Entity Framework POCOs
/// </summary>
public static class EntityExtensions
{
	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<string> query)
		=> query
			.OrderBy(s => s)
			.Select(s => new SelectListItem
			{
				Text = s,
				Value = s
			})
			.ToListAsync();

	public static List<SelectListItem> ToDropDownList(this IEnumerable<SystemsResponse> list)
		=> [.. list
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Text = s.Code,
				Value = s.Code
			})];

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<string> strings)
		=> strings.Select(s => new SelectListItem
		{
			Text = s,
			Value = s
		});

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<PermissionTo> permissions)
		=> permissions.Select(p => new SelectListItem
		{
			Text = p.ToString().SplitCamelCase(),
			Value = ((int)p).ToString()
		});

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<int> ints)
		=> ints.Select(i => new SelectListItem
		{
			Text = i.ToString(),
			Value = i.ToString()
		});

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<Genre> query)
		=> query
			.OrderBy(g => g.DisplayName)
			.Select(g => new SelectListItem
			{
				Text = g.DisplayName,
				Value = g.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<GameGroup> query)
		=> query
			.OrderBy(g => g.Name)
			.Select(g => new SelectListItem
			{
				Text = g.Name,
				Value = g.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<GameSystem> query)
		=> query
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Text = s.Code,
				Value = s.Code
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownListWithId(this IQueryable<GameSystem> query)
		=> query
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Text = s.Code,
				Value = s.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<PublicationClass> query)
		=> query
			.OrderBy(p => p.Name)
			.Select(p => new SelectListItem
			{
				Text = p.Name,
				Value = p.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<SubmissionRejectionReason> query)
		=> query
			.OrderBy(r => r.DisplayName)
			.Select(r => new SelectListItem
			{
				Text = r.DisplayName,
				Value = r.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<GameSystemFrameRate> query, int systemId)
		=> query
			.ForSystem(systemId)
			.OrderBy(fr => fr.Obsolete)
			.ThenBy(fr => fr.RegionCode)
			.ThenBy(fr => fr.FrameRate)
			.Select(g => new SelectListItem
			{
				Text = g.RegionCode + " " + g.FrameRate + (g.Obsolete ? " (Obsolete)" : ""),
				Value = g.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<Flag> query, IEnumerable<PermissionTo> userPermissions)
		=> query
			.OrderBy(f => f.Name)
			.Select(f => new SelectListItem
			{
				Text = f.Name,
				Value = f.Id.ToString(),
				Disabled = f.PermissionRestriction.HasValue
					&& !userPermissions.Contains(f.PermissionRestriction.Value)
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<Tag> query)
		=> query
			.OrderBy(t => t.DisplayName)
			.Select(t => new SelectListItem
			{
				Text = t.DisplayName,
				Value = t.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<Publication> query)
		=> query
			.OrderBy(p => p.Title)
			.Select(p => new SelectListItem
			{
				Text = p.Title,
				Value = p.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<ForumCategory> query)
		=> query.Select(c => new SelectListItem
		{
			Text = c.Title,
			Value = c.Id.ToString()
		}).ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<Forum> query, bool canSeeRestricted, int forumId)
		=> query
			.ExcludeRestricted(canSeeRestricted)
			.Select(f => new SelectListItem
			{
				Text = f.Name,
				Value = f.Id.ToString(),
				Selected = f.Id == forumId
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<ForumTopic> query, bool canSeeRestricted)
		=> query
			.ExcludeRestricted(canSeeRestricted)
			.Select(t => new SelectListItem
			{
				Text = t.Title,
				Value = t.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<User> query)
		=> query
			.OrderBy(u => u.UserName)
			.Select(u => new SelectListItem
			{
				Text = u.UserName,
				Value = u.Id.ToString()
			})
			.ToListAsync();

	public static Task<List<SelectListItem>> ToDropdownList(this IQueryable<Award> query, int year)
		=> query
			.OrderBy(a => a.Description)
			.Select(a => new SelectListItem
			{
				Text = a.Description + " for " + year,
				Value = a.ShortName
			})
			.ToListAsync();

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<AssignableRole> roles)
		=> roles.Select(r => new SelectListItem
		{
			Text = r.Name,
			Value = r.Id.ToString(),
			Disabled = r.Disabled
		});

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<GameVersion> query, int? systemId = null, int? gameId = null)
	{
		if (systemId.HasValue)
		{
			query = query.ForSystem(systemId.Value);
		}

		if (gameId.HasValue)
		{
			query = query.ForGame(gameId.Value);
		}

		return query
			.OrderBy(v => v.Name)
			.Select(v => new SelectListItem
			{
				Text = v.Name,
				Value = v.Id.ToString()
			})
			.ToListAsync();
	}

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<Game> query, int? systemId = null)
	{
		if (systemId.HasValue)
		{
			query = query.ForSystem(systemId.Value);
		}

		return query
			.OrderBy(g => g.DisplayName)
			.Select(g => new SelectListItem
			{
				Text = g.DisplayName,
				Value = g.Id.ToString()
			})
			.ToListAsync();
	}

	public static Task<List<SelectListItem>> ToDropDownList(this IQueryable<GameGoal> query, int? gameId = null)
	{
		if (gameId.HasValue)
		{
			query = query.ForGame(gameId.Value);
		}

		return query
			.OrderBy(gg => gg.DisplayName)
			.Select(gg => new SelectListItem
			{
				Text = gg.DisplayName,
				Value = gg.Id.ToString()
			})
			.ToListAsync();
	}

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<Tag> tags)
		=> tags
			.OrderBy(t => t.DisplayName)
			.Select(t => new SelectListItem
			{
				Text = t.DisplayName,
				Value = t.Code.ToLower()
			});

	public static IEnumerable<SelectListItem> ToDropDown(this IEnumerable<Flag> flags)
		=> flags
			.OrderBy(f => f.Token)
			.Select(f => new SelectListItem
			{
				Text = f.Name,
				Value = f.Token.ToLower()
			});

	public static List<SelectListItem> ToDropDown<T>(this IEnumerable<T> enums)
		where T : Enum
		=> [.. enums
			.Select(e => new SelectListItem
			{
				Value = ((int)(object)e).ToString(),
				Text = e.EnumDisplayName()
			})
			.OrderBy(s => s.Text)];

	public static List<SelectListItem> WithDefaultEntry(this IEnumerable<SelectListItem> items)
		=> [.. UiDefaults.DefaultEntry, .. items];

	public static List<SelectListItem> WithAnyEntry(this IEnumerable<SelectListItem> items)
		=> [.. UiDefaults.AnyEntry, .. items];

	public static IQueryable<Pages.Submissions.IndexModel.SubmissionEntry> ToSubListEntry(this IQueryable<Submission> query, int? userIdForVotes = null)
		=> query.Select(s => new Pages.Submissions.IndexModel.SubmissionEntry
		{
			Id = s.Id,
			System = s.System != null ? s.System!.Code : "Unknown",
			Game = s.GameVersion != null && !string.IsNullOrWhiteSpace(s.GameVersion.TitleOverride) ? s.GameVersion.TitleOverride : s.Game != null ? s.Game.DisplayName : s.GameName,
			Frames = s.Frames,
			FrameRate = s.SystemFrameRateId != null ? s.SystemFrameRate!.FrameRate : 60,
			Goal = s.GameGoal != null ? s.GameGoal.DisplayName : s.Branch,
			By = s.SubmissionAuthors.OrderBy(sa => sa.Ordinal).Select(sa => sa.Author!.UserName).ToList(),
			AdditionalAuthors = s.AdditionalAuthors,
			Date = s.CreateTimestamp,
			Status = s.Status,
			Judge = s.Judge != null ? s.Judge.UserName : null,
			Publisher = s.Publisher != null ? s.Publisher.UserName : null,
			IntendedClass = s.IntendedClass != null ? s.IntendedClass.Name : null,
			SyncedOn = s.SyncedOn,
			Votes = s.Topic != null && s.Topic.Poll != null
				&& s.Topic.Poll.PollOptions.Any(o => o.Text == SiteGlobalConstants.PollOptionYes)
				&& s.Topic.Poll.PollOptions.Any(o => o.Text == SiteGlobalConstants.PollOptionsMeh)
				&& s.Topic.Poll.PollOptions.Any(o => o.Text == SiteGlobalConstants.PollOptionNo)
				? new VoteCounts
				{
					VotesYes = s.Topic.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionYes).Votes.Count,
					VotesMeh = s.Topic.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionsMeh).Votes.Count,
					VotesNo = s.Topic.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionNo).Votes.Count,
					UserVotedYes = s.Topic.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionYes).Votes.Any(v => v.UserId == userIdForVotes),
					UserVotedMeh = s.Topic.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionsMeh).Votes.Any(v => v.UserId == userIdForVotes),
					UserVotedNo = s.Topic.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionNo).Votes.Any(v => v.UserId == userIdForVotes),
				}
			: null,
		});

	public static IQueryable<TASVideos.Pages.Submissions.EditModel.SubmissionEdit> ToSubmissionEditModel(this IQueryable<Submission> query)
		=> query.Select(s => new TASVideos.Pages.Submissions.EditModel.SubmissionEdit
		{
			GameName = s.GameName ?? "",
			GameVersion = s.SubmittedGameVersion,
			RomName = s.RomName,
			Goal = s.Branch,
			Emulator = s.EmulatorVersion,
			SubmitDate = s.CreateTimestamp,
			Submitter = s.Submitter!.UserName,
			Status = s.Status,
			EncodeEmbedLink = s.EncodeEmbedLink,
			Judge = s.Judge != null ? s.Judge.UserName : "",
			Publisher = s.Publisher != null ? s.Publisher.UserName : "",
			IntendedPublicationClass = s.IntendedClassId,
			RejectionReason = s.RejectionReasonId,
			ExternalAuthors = s.AdditionalAuthors,
			Title = s.Title,
			Authors = s.SubmissionAuthors
				.OrderBy(sa => sa.Ordinal)
				.Select(sa => sa.Author!.UserName)
				.ToList()
		});

	public static IQueryable<Pages.Submissions.ViewModel.SubmissionDisplay> ToSubmissionDisplayModel(this IQueryable<Submission> query)
		=> query.Select(s => new Pages.Submissions.ViewModel.SubmissionDisplay
		{
			StartType = (MovieStartType?)s.MovieStartType,
			SystemDisplayName = s.System!.DisplayName,
			GameName = s.GameId != null ? s.Game!.DisplayName : null,
			SubmittedGameName = s.GameName,
			GameVersion = s.GameVersionId != null ? s.GameVersion!.Name : "",
			SubmittedGameVersion = s.SubmittedGameVersion,
			SubmittedRomName = s.RomName,
			SubmittedBranch = s.Branch,
			Goal = s.GameGoal != null
				? s.GameGoal!.DisplayName
				: null,
			Emulator = s.EmulatorVersion,
			FrameCount = s.Frames,
			FrameRate = s.SystemFrameRate!.FrameRate,
			RerecordCount = s.RerecordCount,
			Date = s.CreateTimestamp,
			Submitter = s.Submitter!.UserName,
			Status = s.Status,
			EncodeEmbedLink = s.EncodeEmbedLink,
			Judge = s.Judge != null ? s.Judge.UserName : "",
			Title = s.Title,
			ClassName = s.IntendedClass != null ? s.IntendedClass.Name : "",
			Publisher = s.Publisher != null ? s.Publisher.UserName : "",
			SystemId = s.SystemId,
			SystemFrameRateId = s.SystemFrameRateId,
			GameId = s.GameId,
			GameVersionId = s.GameVersionId,
			RejectionReasonDisplay = s.RejectionReasonId.HasValue
				? s.RejectionReason!.DisplayName
				: null,
			Authors = s.SubmissionAuthors
				.OrderBy(sa => sa.Ordinal)
				.Select(sa => sa.Author!.UserName)
				.ToList(),
			AdditionalAuthors = s.AdditionalAuthors,
			TopicId = s.TopicId,
			Warnings = s.Warnings,
			CycleCount = s.CycleCount,
			Annotations = s.Annotations,
			GameGoalId = s.GameGoalId,
			SyncedOn = s.SyncedOn,
			SyncedBy = s.SyncedByUser != null ? s.SyncedByUser.UserName : null,
			AdditionalSyncNotes = s.AdditionalSyncNotes,
			HashType = s.HashType,
			Hash = s.Hash
		});

	public static IQueryable<Pages.Publications.IndexModel.PublicationDisplay> ToViewModel(this IQueryable<Publication> query, bool ratingSort = false, int userId = -1)
	{
		var q = query
			.Select(p => new Pages.Publications.IndexModel.PublicationDisplay
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
					.Select(pu => new Pages.Publications.IndexModel.PublicationDisplay.PublicationUrl(pu.Type, pu.Url!, pu.DisplayName))
					.ToList(),
				TopicId = p.Submission!.TopicId ?? 0,
				EmulatorVersion = p.EmulatorVersion,
				Tags = p.PublicationTags
					.Select(pt => new Pages.Publications.IndexModel.PublicationDisplay.Tag(
						pt.Tag!.DisplayName,
						pt.Tag.Code)),
				GameGenres = p.Game!.GameGenres.Select(gg => gg.Genre!.DisplayName),
				Files = p.Files
					.Select(f => new Pages.Publications.IndexModel.PublicationDisplay.File(
						f.Id,
						f.Path,
						f.Type,
						f.Description)),
				Flags = p.PublicationFlags
					.Select(pf => new Pages.Publications.IndexModel.PublicationDisplay.Flag(
						pf.Flag!.IconPath,
						pf.Flag!.LinkPath,
						pf.Flag.Name)),
				ObsoletedMovies = p.ObsoletedMovies
					.Select(o => new Pages.Publications.IndexModel.PublicationDisplay.ObsoleteMovie(o.Id, o.Title)),
				RatingCount = p.PublicationRatings.Count,
				OverallRating = p.PublicationRatings
					.Where(pr => !pr.Publication!.Authors.Select(a => a.UserId).Contains(pr.UserId))
					.Where(pr => pr.User!.UseRatings)
					.Average(pr => pr.Value),
				Rating = new Pages.Publications.IndexModel.PublicationDisplay.CurrentRating(
					p.PublicationRatings.Where(pr => pr.UserId == userId).Select(pr => pr.Value.ToString()).FirstOrDefault(),
					p.PublicationRatings.All(pr => pr.UserId != userId))
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
		=> roles.Select(r => new ListModel.RoleDisplay(
			r.IsDefault,
			r.Id,
			r.Name,
			r.Description,
			r.RolePermission.Select(rp => rp.PermissionId).ToList(),
			r.RoleLinks.Select(rl => rl.Link).ToList(),
			r.UserRole.Select(ur => ur.User!.UserName).ToList()));

	public static IQueryable<Pages.UserFiles.InfoModel.UserFileModel> ToUserFileModel(this IQueryable<UserFile> userFiles, bool hideComments = true)
		=> userFiles.Select(uf => new Pages.UserFiles.InfoModel.UserFileModel
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
				.Select(c => new Pages.UserFiles.InfoModel.UserFileModel.Comment(c.Id, c.Text, c.CreationTimeStamp, c.UserId, c.User!.UserName))
				.ToList(),
			HideComments = hideComments,
			Annotations = uf.Annotations
		});

	public static IQueryable<Pages.UserFiles.IndexModel.UserMovie> ToUserMovieListModel(this IQueryable<UserFile> userFiles)
		=> userFiles.Select(uf => new Pages.UserFiles.IndexModel.UserMovie(
			uf.Id,
			uf.Author!.UserName,
			uf.UploadTimestamp,
			uf.FileName,
			uf.Title));

	public static IQueryable<DisplayMiniMovie.MiniMovieModel> ToMiniMovieModel(this IQueryable<Publication> publications)
		=> publications.Select(p => new DisplayMiniMovie.MiniMovieModel
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

	public static IQueryable<Pages.Submissions.PublishModel.SubmissionPublishModel> ToPublishModel(this IQueryable<Submission> submissions)
		=> submissions.Select(s => new Pages.Submissions.PublishModel.SubmissionPublishModel
		{
			System = s.System!.Code,
			Region = s.SystemFrameRate!.RegionCode + " " + s.SystemFrameRate.FrameRate,
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
			Goal = s.GameGoal != null ? s.GameGoal.DisplayName : s.Branch,
			GameGoalId = s.GameGoalId
		});

	public static IQueryable<AddEditModel.RoleEdit> ToRoleEditModel(this IQueryable<Role> query)
		=> query.Select(r => new AddEditModel.RoleEdit
		{
			Name = r.Name,
			IsDefault = r.IsDefault,
			Description = r.Description,
			AutoAssignPostCount = r.AutoAssignPostCount,
			AutoAssignPublications = r.AutoAssignPublications,
			RelatedLinks = r.RoleLinks
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

	public static IQueryable<Pages.UserFiles.Uncataloged.UncatalogedViewModel> ToUnCatalogedModel(this IQueryable<UserFile> query)
		=> query.Select(uf => new Pages.UserFiles.Uncataloged.UncatalogedViewModel(
			uf.Id,
			uf.FileName,
			uf.System != null ? uf.System.Code : null,
			uf.UploadTimestamp,
			uf.Author!.UserName));

	public static IQueryable<LatestModel.LatestPost> ToLatestPost(this IQueryable<ForumPost> query)
		=> query.Select(p => new LatestModel.LatestPost(
			p.CreateTimestamp,
			p.Id,
			p.TopicId ?? 0,
			p.Topic!.Title,
			p.Topic.ForumId,
			p.Topic!.Forum!.Name,
			p.Text,
			p.Poster!.UserName));
}
