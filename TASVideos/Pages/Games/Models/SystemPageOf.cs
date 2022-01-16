using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TASVideos.Core;

namespace TASVideos.Pages.Games.Models
{
	public class SystemPageOf<T> : PageOf<T>
	{
		public SystemPageOf(IEnumerable<T> items)
			: base(items)
		{
		}

		[Display(Name = "System")]
		public string? SystemCode { get; set; }

		public string? SearchTerms { get; set; }

		public static new SystemPageOf<T> Empty() => new (Enumerable.Empty<T>());
	}
}
