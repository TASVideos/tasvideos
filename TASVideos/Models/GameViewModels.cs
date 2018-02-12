using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TASVideos.Data.Entity.Game;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a <see cref="Game"/> for the purpose of displaying
	/// on a dedicated page
	/// </summary>
	public class GameViewModel
	{
		public int Id { get; set; }
		public string DisplayName { get; set; }
		public string Abbreviation { get; set; }
		public string ScreenshotUrl { get; set; }
		public string SystemCode { get; set; }
	}
}
