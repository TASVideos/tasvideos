using System.Collections.Generic;

using TASVideos.Data.Entity.Awards;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents all the data necessary for the Awards module for a year
	/// </summary>
	public class AwardByYearModel
	{
		public AwardType Type { get; set; }
		public int Year { get; set; }
		public string ShortName { get; set; }
		public string Description { get; set; }

		public IEnumerable<int> Movies { get; set; } = new List<int>();
		public IEnumerable<User> Users { get; set; } = new List<User>();

		public class User
		{
			public int Id { get; set; }
			public string UserName { get; set; }
		}
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
