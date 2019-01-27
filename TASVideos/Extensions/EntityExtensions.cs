using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;

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

		public static IQueryable<Submission> SearchBy(this IQueryable<Submission> query, SubmissionSearchRequest criteria)
		{
			if (!string.IsNullOrWhiteSpace(criteria.User))
			{
				query = query.Where(s => s.SubmissionAuthors.Any(sa => sa.Author.UserName == criteria.User)
					|| s.Submitter.UserName == criteria.User);
			}

			if (criteria.Cutoff.HasValue)
			{
				query = query.Where(s => s.CreateTimeStamp >= criteria.Cutoff.Value);
			}

			if (criteria.StatusFilter.Any())
			{
				query = query.Where(s => criteria.StatusFilter.Contains(s.Status));
			}

			if (criteria.Limit.HasValue)
			{
				query = query.Take(criteria.Limit.Value);
			}

			return query;
		}
	}
}
