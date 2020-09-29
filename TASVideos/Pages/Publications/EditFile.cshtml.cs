using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationFiles)]
	public class EditFileModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IMediaFileUploader _uploader;
		public EditFileModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IMediaFileUploader uploader)
		{
			_db = db;
			_publisher = publisher;
			_uploader = uploader;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public string Title { get; set; } = "";

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

			string? publisherMessage = null;
			if (Files.UseNewScreenshot && Files.NewScreenshot != null)
			{
				if (screenshot != null)
				{
					_db.PublicationFiles.Remove(screenshot);
				}

				await _uploader.UploadScreenshot(Id, Files.NewScreenshot, Files.ScreenshotDescription);
				publisherMessage = $"Publication {Id} {Title} Added screenshot ({Files.NewScreenshot.Length} bytes)";
			}
			else
			{
				if (screenshot != null)
				{
					screenshot.Description = Files.ScreenshotDescription;
					publisherMessage = $"Publication {Id} {Title} Updated screenshot description: {screenshot.Description}";
				}
			}

			if (publisherMessage != null)
			{
				_publisher.SendPublicationEdit(publisherMessage, $"{Id}M", User.Identity.Name!);
			}

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("Edit", new { Id });
		}

		public class PublicationFilesEditModel
		{
			[StringLength(250)]
			public string? ScreenshotDescription { get; set; }

			public bool UseNewScreenshot { get; set; }
			public IFormFile? NewScreenshot { get; set; }
			public string ExistingScreenshotName { get; set; } = "";
		}
	}
}
