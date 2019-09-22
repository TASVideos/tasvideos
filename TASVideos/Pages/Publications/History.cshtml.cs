using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class HistoryModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public HistoryModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var publication = await _db.Publications
				.ProjectTo<PublicationDisplayModel>()
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (publication == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
