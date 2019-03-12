using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

		public static async Task<IEnumerable<SubmissionListEntry>> PersistToSubListEntry(this IQueryable<Submission> query)
		{
			var iquery = query
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author);

			// It is important to actually query for an Entity object here instead of a ViewModel
			// Because we need the title property which is a derived property that can't be done in Linq to Sql
			// And needs a variety of information from sub-tables, hence all the includes
			var results = await iquery.ToListAsync();

			return results
				.Select(s => new SubmissionListEntry
				{
					Id = s.Id,
					System = s.System.Code,
					GameName = s.GameName,
					Time = s.Time(),
					Branch = s.Branch,
					Author = string.Join(" & ", s.SubmissionAuthors.Select(sa => sa.Author.UserName).ToList()),
					Submitted = s.CreateTimeStamp,
					Status = s.Status
				});
		}
	}
}
