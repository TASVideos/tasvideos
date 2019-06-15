using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.CreateAdditionalMovieFiles)]
	public class AdditionalMoviesModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public AdditionalMoviesModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public string PublicationTitle { get; set; }

		public ICollection<PublicationFileModel> AvailableMovieFiles { get; set; } = new List<PublicationFileModel>();

		[BindProperty]
		[Display(Name = "Add an additional movie file:", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile AdditionalMovieFile { get; set; }


		public async Task<IActionResult> OnGet()
		{
			var publication = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new { p.Id, p.Title })
				.SingleOrDefaultAsync();

			if (publication == null)
			{
				return NotFound();
			}

			PublicationTitle = publication.Title;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			var publication = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new { p.Id, p.Title })
				.SingleOrDefaultAsync();

			if (publication == null)
			{
				return NotFound();
			}

			if (!AdditionalMovieFile.FileName.EndsWith(".zip")
					|| AdditionalMovieFile.ContentType != "application/x-zip-compressed")
				{
					ModelState.AddModelError(nameof(AdditionalMovieFile), "Not a valid .zip file");
				}

				if (AdditionalMovieFile.Length > 150 * 1024)
				{
					ModelState.AddModelError(
						nameof(AdditionalMovieFile),
						".zip is too big, are you sure this is a valid movie file?");
				}

			if (!ModelState.IsValid)
			{
				PublicationTitle = publication.Title;
				await PopulateAvailableMovieFiles();
				return Page();
			}

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateAvailableMovieFiles()
		{
			AvailableMovieFiles = await _db.PublicationFiles
				.ThatAreMovieFiles()
				.ForPublication(Id)
				.Select(pf => new PublicationFileModel
				{
					Id = pf.Id,
					FileName = pf.Path
				})
				.ToListAsync();
		}

		public class PublicationFileModel
		{
			public int Id { get; set; }
			public string FileName { get; set; }
		}
	}
}
