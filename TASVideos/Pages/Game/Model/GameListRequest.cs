using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;

namespace TASVideos.Pages.Game.Model
{
	public class GameListRequest : PagedModel
	{
		public GameListRequest()
		{
			PageSize = 25;
		}

		public string SystemCode { get; set; }
	}

	public class SystemPageOf<T> : PagedModel, IEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public SystemPageOf(IEnumerable<T> items)
		{
			_items = items;
		}

		[Display(Name = "System")]
		public string SystemCode { get; set; }

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}
}
