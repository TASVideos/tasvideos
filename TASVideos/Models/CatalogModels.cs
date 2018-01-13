using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;

namespace TASVideos.Models
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

	public class GameEditModel
	{
		public int? Id { get; set; }

		[Required]
		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Required]
		[Display(Name = "Good Name")]
		public string GoodName { get; set; }

		[Required]
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		[Required]
		[Display(Name = "Abbreviation")]
		public string Abbreviation { get; set; }

		[Required]
		[Display(Name = "Search Key")]
		public string SearchKey { get; set; }

		[Required]
		[Display(Name = "Youtube Tags")]
		public string YoutubeTags { get; set; }

		[Display(Name = "Screenshot Url")]
		public string ScreenshotUrl { get; set; }

		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
	}
}
