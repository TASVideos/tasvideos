using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity
{
	[AllowAnonymous]
	public class PublishersModel : PageModel
	{
		private readonly ApplicationDbContext _db;

		public PublishersModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public IEnumerable<MovieEntryModel> Publications { get; set; } = new List<MovieEntryModel>();

		[FromRoute]
		public string UserName { get; set; }

		public async Task OnGet()
		{
			Publications = await _db.Publications
				.Where(s => s.CreateUserName == UserName)
				.Select(s => new MovieEntryModel
				{
					Id = s.Id,
					Title = s.Title
				})
				.ToListAsync();
		}
	}
}
