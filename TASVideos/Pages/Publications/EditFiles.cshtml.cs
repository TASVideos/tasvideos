using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationFiles)]
	public class EditFilesModel : BasePageModel
	{
		private static readonly List<FileType> PublicationFileTypes = Enum
			.GetValues(typeof(FileType))
			.Cast<FileType>()
			.ToList();

		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IMediaFileUploader _uploader;
		public EditFilesModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IMediaFileUploader uploader)
		{
			_db = db;
			_publisher = publisher;
			_uploader = uploader;
		}

		public IEnumerable<SelectListItem> AvailableTypes =
			PublicationFileTypes
				.Where(t => t != FileType.MovieFile)
				.Select(t => new SelectListItem
				{
					Text = t.ToString(),
					Value = ((int)t).ToString()
				});

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public string Title { get; set; } = "";

		public ICollection<PublicationFile> Files { get; set; } = new List<PublicationFile>();

		[Required]
		[BindProperty]
		public IFormFile? NewFile { get; set; }

		[Required]
		[BindProperty]
		public FileType Type { get; set; }

		[BindProperty]
		[StringLength(250)]
		public string? Description { get; set; }

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
				.ToListAsync();

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				Files = await _db.PublicationFiles
					.Where(f => f.PublicationId == Id)
					.ToListAsync();

				return Page();
			}

			string path;
			if (Type == FileType.Screenshot)
			{
				path = await _uploader.UploadScreenshot(Id, NewFile!, Description);
			}
			else
			{
				path = await _uploader.UploadTorrent(Id, NewFile!);
			}

			_publisher.SendPublicationEdit(
				$"Publication {Id} {Title} added {Type} file {path}",
				$"{Id}M",
				User.Identity.Name!);

			return RedirectToPage("EditFiles", new { Id });
		}

		public async Task<IActionResult> OnPostDelete(int publicationFileId)
		{
			var file = await _uploader.DeleteFile(publicationFileId);

			if (file != null)
			{
				_publisher.SendPublicationEdit(
					$"Publication {Id} deleted {file.Type} file {file.Path}",
					$"{Id}M",
					User.Identity.Name!);
			}

			return RedirectToPage("EditFiles", new { Id });
		}
	}
}
