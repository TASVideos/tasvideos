using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Publications.Models;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Extensions;

/// <summary>
/// Web front-end specific extension methods for Entity Framework POCOs
/// </summary>
public static class EntityExtensions
{
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
				GenreTags = p.Game!.GameGenres
					.Select(gg => new PublicationDisplayModel.TagModel
					{
						DisplayName = gg.Genre!.DisplayName,
						Code = gg.Genre.DisplayName // TODO
					}),
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
				Region = p.Rom != null ? p.Rom.Region : null,
				RomVersion = p.Rom != null ? p.Rom.Version : null
			});

		if (ratingSort)
		{
			q = q.OrderByDescending(p => p.OverallRating);
		}

		return q;
	}
}
