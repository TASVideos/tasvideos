using System.Collections.Generic;

using TASVideos.Data.Entity.Awards;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents an award and related data for the Awards module
	/// </summary>
	public class AwardDetailsModel
	{
		public string ShortName { get; set; }
		public string Description { get; set; }
		public int Year { get; set; }
		public AwardType Type { get; set; }
		public IEnumerable<int> PublicationIds { get; set; } = new HashSet<int>();
		public IEnumerable<int> UserIds { get; set; } = new HashSet<int>();
	}

	/// <summary>
	/// Represents the data necessary to show an award image
	/// </summary>
	public class AwardDisplayModel
	{
		public string ShortName { get; set; }
		public string Description { get; set; }
		public int Year { get; set; }
	}
}
