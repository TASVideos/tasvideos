using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			return RedirectToPage("/Profile/UserFiles");
		}
	}
}
