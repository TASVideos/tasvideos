using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationFiles)]
	public class EditFileModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public EditFileModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public string Title { get; set; }

		[BindProperty]
		public IEnumerable<int> FilesToRemove { get; set; } = new List<int>();

		[BindProperty]
		public string ExistingScreenshotDescription { get; set; }

		[BindProperty]
		public IFormFile NewScreenshotFile { get; set; }

		[BindProperty]
		public string NewScreenshotDescription { get; set; }

		[BindProperty]
		public IEnumerable<IFormFile> TorrentFiles { get; set; }

		public IEnumerable<PublicationFileDisplayModel> Files { get; set; } = new List<PublicationFileDisplayModel>();

		public async Task<IActionResult> OnGet()
		{
			Title = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => p.Title)
				.SingleOrDefaultAsync();

			if (Title == null)
			{
				return NotFound();
			}

			Files = await _db.PublicationFiles
				.Where(f => f.PublicationId == Id)
				.ProjectTo<PublicationFileDisplayModel>()
				.ToListAsync();

			// Bind the things
			ExistingScreenshotDescription = Files.FirstOrDefault(f => f.Type == FileType.Screenshot)?.Description;

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				// TODO: repopulate things?
				return Page();
			}

			var exists = await _db.Publications
				.AnyAsync(p => p.Id == Id);

			if (!exists)
			{
				return NotFound();
			}

			var files = await _db.PublicationFiles
				.Where(f => f.PublicationId == Id)
				.ToListAsync();

			var screenshot = files.FirstOrDefault(f => f.Type == FileType.Screenshot);
			if (screenshot != null)
			{
				screenshot.Description = ExistingScreenshotDescription;
			}

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}
	}
}
