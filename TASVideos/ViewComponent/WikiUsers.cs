using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class WikiUsers : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public WikiUsers(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var role = TranslateLegacyName(
				ParamHelper.GetValueFor(pp, "roles"));

			var model = await _db.Users
				.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role))
				.Select(u => new WikiUserEntry
				{
					UserName = u.UserName,
					PublicationCount = u.Publications.Count,
					SubmissionCount = u.Submissions.Count,
				})
				.ToListAsync();

			return View(model);
		}

		private string TranslateLegacyName(string role)
		{
			// Translate legacy names for roles into modern ones
			switch (role)
			{
				default:
					return role;
				case "admin":
					return "Site Admin";
				case "adminassistant":
					return "Admin Assistant";
				case "seniorjudge":
					return "Senior Judge";
				case "senior publisher":
					return "Senior Publisher";
			}
		}
	}
}
