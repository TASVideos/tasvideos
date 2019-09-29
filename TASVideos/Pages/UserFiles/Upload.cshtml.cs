using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.MovieParsers;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles
{
	[RequirePermission(PermissionTo.UploadUserFiles)]
	public class UploadModel : BasePageModel
	{
		private static readonly string[] SupportedCompressionTypes = { ".zip" }; // TODO: remaining format types
		private static readonly string[] SupportedSupplementalTypes = { ".lua", ".wch", ".gst" };
		private readonly ApplicationDbContext _db;
		private readonly MovieParser _parser;

		public UploadModel(ApplicationDbContext db, MovieParser parser)
		{
			_db = db;
			_parser = parser;
		}

		[BindProperty]
		public UserFileUploadModel UserFile { get; set; }

		public int StorageUsed { get; set; } 

		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

		public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

		public async Task OnGet()
		{
			await Initialize();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await Initialize();
				return Page();
			}

			var fileExt = Path.GetExtension(UserFile.File.FileName);


			if (!SupportedCompressionTypes.Contains(fileExt)
				&& !SupportedCompressionTypes.Contains(fileExt)
				&& !_parser.SupportedMovieExtensions.Contains(fileExt))
			{
				ModelState.AddModelError(
					$"{nameof(UserFile)}.{nameof(UserFile.File)}",
					$"Unsupported file type: {fileExt}");
				await Initialize();
				return Page();
			}

			var fileBytes = await FormFileToBytes(UserFile.File);

			if (SupportedCompressionTypes.Contains(fileExt))
			{
				// TODO
				ModelState.AddModelError(
					$"{nameof(UserFile)}.{nameof(UserFile.File)}",
					$"Compressed files not yet supported");
				await Initialize();
				return Page();
			}

			var supportedExtensions = _parser.SupportedMovieExtensions;
			if (_parser.SupportedMovieExtensions.Contains(fileExt))
			{
				//_parser.Parse()
			}

			var userFile = new UserFile
			{
				Id = DateTime.UtcNow.Ticks,
				Title = UserFile.Title,
				Description = UserFile.Description,
				SystemId = UserFile.SystemId,
				GameId = UserFile.GameId,
				AuthorId = User.GetUserId(),
				LogicalLength = (int)UserFile.File.Length,
				UploadTimestamp = DateTime.UtcNow
			};

			_db.UserFiles.Add(userFile);
			await _db.SaveChangesAsync();

			return RedirectToPage("/Profile/UserFiles");
		}

		private async Task Initialize()
		{
			var userId = User.GetUserId();
			StorageUsed = await _db.UserFiles
				.Where(uf => uf.AuthorId == userId)
				.SumAsync(uf => uf.LogicalLength);

			AvailableSystems = UiDefaults.DefaultEntry.Concat(await _db.GameSystems
				.Select(s => new SelectListItem
				{
					Value = s.Id.ToString(),
					Text = s.Code
				})
				.ToListAsync());

			AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
				.OrderBy(g => g.SystemId)
				.ThenBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync());
		}
	}
}
