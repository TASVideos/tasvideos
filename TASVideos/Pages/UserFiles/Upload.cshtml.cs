using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
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

		public UserFileUploadModel UserFile { get; set; }

		public int StorageUsed { get; set; } 

		public async Task OnGet()
		{
			await CalculateStorageUsed();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await CalculateStorageUsed();
				return Page();
			}

			return RedirectToPage("/Profile/UserFiles");
		}

		private async Task CalculateStorageUsed()
		{
			var userId = User.GetUserId();
			StorageUsed = await _db.UserFiles
				.Where(uf => uf.AuthorId == userId)
				.SumAsync(uf => uf.LogicalLength);
		}
	}
}
