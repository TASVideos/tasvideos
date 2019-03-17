using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private static readonly IEnumerable<SelectListItem> Statuses = Enum.GetValues(typeof(SubmissionStatus))
			.Cast<SubmissionStatus>()
			.Select(s => new SelectListItem
			{
				Text = s.EnumDisplayName(),
				Value = ((int)s).ToString()
			})
			.ToList();

		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromQuery]
		public SubmissionSearchRequest Search { get; set; } = new SubmissionSearchRequest();

		public SubmissionPageOf<SubmissionListEntry> Submissions { get; set; }

		[Display(Name = "Statuses")]
		public IEnumerable<SelectListItem> AvailableStatuses => Statuses;

		public IEnumerable<SelectListItem> SystemList { get; set; } = new List<SelectListItem>();

		public async Task OnGet()
		{
			SystemList = UiDefaults.DefaultEntry.Concat(
				await _db.GameSystems
				.ToDropdown()
				.ToListAsync());

			// Defaults
			if (!Search.StatusFilter.Any())
			{
				Search.StatusFilter = !string.IsNullOrWhiteSpace(Search.User) || Search.Years.Any()
					? SubmissionSearchRequest.All
					: SubmissionSearchRequest.Default;
			}

			var entries = await _db.Submissions
				.FilterBy(Search)
				.ToSubListEntry()
				.SortedPageOf(_db, Search);

			Submissions = new SubmissionPageOf<SubmissionListEntry>(entries)
			{
				PageSize = entries.PageSize,
				CurrentPage = entries.CurrentPage,
				RowCount = entries.RowCount,
				Sort = entries.Sort,
				Years = Search.Years,
				StatusFilter = Search.StatusFilter,
				System = Search.System,
				User = Search.User
			};
		}

		public async Task<IActionResult> OnGetSearchAuthor(string partial)
		{
			if (string.IsNullOrWhiteSpace(partial) || partial.Length < 3)
			{
				return new JsonResult(new string[0]);
			}

			var upper = partial.ToUpper();
			var result = await _db.Users
				.ThatHaveSubmissions()
				.ThatPartiallyMatch(upper)
				.Select(u => u.UserName)
				.ToListAsync();

			return new JsonResult(result);
		}
	}
}
