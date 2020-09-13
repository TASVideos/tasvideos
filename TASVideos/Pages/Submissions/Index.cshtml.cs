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

		// For legacy routes such as Subs-Rej-422up
		[FromRoute]
		public string? Query { get; set; }

		[FromQuery]
		public SubmissionSearchRequest Search { get; set; } = new SubmissionSearchRequest();

		public SubmissionPageOf<SubmissionListEntry> Submissions { get; set; } = SubmissionPageOf<SubmissionListEntry>.Empty();

		[Display(Name = "Statuses")]
		public IEnumerable<SelectListItem> AvailableStatuses => Statuses;

		public IEnumerable<SelectListItem> SystemList { get; set; } = new List<SelectListItem>();

		public async Task OnGet()
		{
			SystemList = UiDefaults.DefaultEntry.Concat(
				await _db.GameSystems
				.ToDropdown()
				.ToListAsync());

			var search = ToSearchRequest(Query);
			if (search != null)
			{
				Search = search;
			}

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
				.SortedPageOf(Search);

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

		private static SubmissionSearchRequest? ToSearchRequest(string? query)
		{
			var tokens = query.ToTokens();

			if (!tokens.Any())
			{
				return null;
			}

			var request = new SubmissionSearchRequest();

			var statuses = new List<SubmissionStatus>();
			foreach (var kvp in StatusTokenMapping)
			{
				if (tokens.Any(t => t == kvp.Key))
				{
					statuses.Add(kvp.Value);
				}
			}

			if (statuses.Any())
			{
				request.StatusFilter = statuses;
			}

			if (tokens.Any(t => t.EndsWith("up")))
			{
				request.User = tokens.First(t => t.EndsWith("up"));
			}

			var years = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1);
			request.Years = years.Where(y => tokens.Contains("y" + y));

			return request;
		}

		

		private static Dictionary<string, SubmissionStatus> StatusTokenMapping = new Dictionary<string, SubmissionStatus>
		{
			["new"] = SubmissionStatus.New,
			["can"] = SubmissionStatus.Cancelled,
			["inf"] = SubmissionStatus.NeedsMoreInfo,
			["del"] = SubmissionStatus.Delayed,
			["jud"] = SubmissionStatus.JudgingUnderWay,
			["acc"] = SubmissionStatus.Accepted,
			["und"] = SubmissionStatus.PublicationUnderway,
			["pub"] = SubmissionStatus.Published,
			["rej"] = SubmissionStatus.Rejected
		};
	}
}
