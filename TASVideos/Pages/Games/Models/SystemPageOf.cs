using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;

namespace TASVideos.Pages.Games.Models
{
	public class SystemPageOf<T> : PageOf<T>
	{
		public SystemPageOf(IEnumerable<T> items)
			: base(items)
		{
		}

		[Display(Name = "System")]
		public string SystemCode { get; set; }
	}
}
