using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Extensions
{
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
					Value = g.Id.ToString(),
					Text = g.DisplayName
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
	}
}
