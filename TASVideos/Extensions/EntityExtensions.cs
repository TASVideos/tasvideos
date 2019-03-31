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
		public static IQueryable<SelectListItem> ToDropdown(this IQueryable<GameSystem> query)
		{
			return query
				.Select(s => new SelectListItem
				{
					Text = s.Code,
					Value = s.Code
				});
		}

		public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Tier> query)
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

		public static IQueryable<SubmissionListEntry> ToSubListEntry(this IQueryable<Submission> query)
		{
			return query
				.Select(s => new SubmissionListEntry
				{
					Id = s.Id,
					System = s.System.Code,
					GameName = s.GameName,
					Frames = s.Frames,
					FrameRate = s.SystemFrameRate.FrameRate,
					Branch = s.Branch,
					Author = string.Join(" & ", s.SubmissionAuthors.Select(sa => sa.Author.UserName).ToList()),
					Submitted = s.CreateTimeStamp,
					Status = s.Status
				});
		}
	}
}
