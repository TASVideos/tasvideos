using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.WikiUsers)]
	public class WikiUsers : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public WikiUsers(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(string? roles)
		{
			var role = TranslateLegacyName(roles ?? "");

			var model = await _db.Users
				.ThatHaveRole(role)
				.Select(u => new WikiUserEntry
				{
					UserName = u.UserName,
					PublicationCount = u.Publications.Count,
					SubmissionCount = u.Submissions.Count
				})
				.ToListAsync();

			return View(model);
		}

		private static string TranslateLegacyName(string role)
		{
			// Translate legacy names for roles into modern ones
			return role switch
			{
				"admin" => "Site Admin",
				"adminassistant" => "Admin Assistant",
				"seniorjudge" => "Senior Judge",
				"seniorpublisher" => "Senior Publisher",
				_ => role
			};
		}
	}
}
