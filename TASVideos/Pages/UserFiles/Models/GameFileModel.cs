using System.Collections.Generic;
using TASVideos.RazorPages.Models;

namespace TASVideos.RazorPages.Pages.UserFiles.Models
{
	public class GameFileModel
	{
		public string SystemCode { get; set; } = "";
		public int GameId { get; set; }
		public string GameName { get; set; } = "";

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();
	}
}
