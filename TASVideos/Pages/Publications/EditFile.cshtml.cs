using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationFiles)]
	public class EditFileModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IHostingEnvironment _env;

		public EditFileModel(ApplicationDbContext db, IHostingEnvironment env)
		{
			_db = db;
			_env = env;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public string Title { get; set; }

		[BindProperty]
		public PublicationFilesEditModel Files { get; set; } = new PublicationFilesEditModel();

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

			var files = await _db.PublicationFiles
				.Where(f => f.PublicationId == Id)
				.ToListAsync();

			// Screenshot
			var screenshot = files.FirstOrDefault(f => f.Type == FileType.Screenshot);

			if (screenshot != null)
			{
				Files.ScreenshotDescription = screenshot.Description;
				Files.ExistingScreenshotName = screenshot.Path;
			}
			

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

			// Screenshot
			var screenshot = files.FirstOrDefault(f => f.Type == FileType.Screenshot);

			if (Files.UseNewScreenshot && Files.NewScreenshot != null)
			{
				if (screenshot != null)
				{
					_db.PublicationFiles.Remove(screenshot);
				}

				await SaveScreenshot(Files.NewScreenshot, Files.ScreenshotDescription, Id);
			}
			else
			{
				if (screenshot != null)
				{
					screenshot.Description = Files.ScreenshotDescription;
				}
			}

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}

		// TODO: make a service for this, and refactor Publish.cshtml.cs to use it
		private async Task SaveScreenshot(IFormFile screenshot, string description, int publicationId)
		{
			using (var memoryStream = new MemoryStream())
			{
				await screenshot.CopyToAsync(memoryStream);
				var screenshotBytes = memoryStream.ToArray();

				string screenshotFileName = $"{publicationId}M{Path.GetExtension(screenshot.FileName)}";
				string screenshotPath = Path.Combine(_env.WebRootPath, "media", screenshotFileName);
				System.IO.File.WriteAllBytes(screenshotPath, screenshotBytes);

				var pubFile = new PublicationFile
				{
					PublicationId = publicationId,
					Path = screenshotFileName,
					Type = FileType.Screenshot,
					Description = description
				};

				_db.PublicationFiles.Add(pubFile);
				await _db.SaveChangesAsync();
			}
		}

		public class PublicationFilesEditModel
		{
			[StringLength(250)]
			public string ScreenshotDescription { get; set; }

			public bool UseNewScreenshot { get; set; }
			public IFormFile NewScreenshot { get; set; }
			public string ExistingScreenshotName { get; set; }
		}
	}
}
