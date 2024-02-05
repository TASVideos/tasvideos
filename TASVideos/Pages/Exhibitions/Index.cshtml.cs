using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Data;

namespace TASVideos.Pages.Exhibitions
{
    public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public void OnGet()
        {
        }
    }
}
