using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles
{
	[RequirePermission(PermissionTo.UploadUserFiles)]
	public class UploadModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public UploadModel(ApplicationDbContext db)
		{
			_db = db;
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

			var userFile = new UserFile
			{
				Id = DateTime.UtcNow.Ticks,
				Title = UserFile.Title,
				Description = UserFile.Description,
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
				.ToDropdown()
				.ToListAsync());

			AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
				.OrderBy(g => g.SystemId)
				.ThenBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync());
		}
	}
}
