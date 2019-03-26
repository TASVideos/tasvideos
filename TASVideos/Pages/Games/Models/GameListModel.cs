using System.ComponentModel.DataAnnotations;
using TASVideos.Data;

namespace TASVideos.Pages.Games.Models
{
	public class GameListModel
	{
		[Sortable]
		[Display(Name = "Id")]
		public int Id { get; set; }

		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Sortable]
		[Display(Name = "Name")]
		public string DisplayName { get; set; }

		// Dummy to generate column header
		[Display(Name = "Actions")]
		public object Actions { get; set; }
	}
}
