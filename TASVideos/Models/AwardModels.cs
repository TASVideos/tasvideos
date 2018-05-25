using System.Collections.Generic;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents all the data necessary for the Awards module
	/// </summary>
	public class AwardsModuleModel
	{
		public Dictionary<int, AwardDisplayModel> AwardsByYear { get; set; } = new Dictionary<int, AwardDisplayModel>();

		public class AwardYearModel
		{
			public string ShortName { get; set; }
			public string Description { get; set; }
			public int Year { get; set; }

			public IEnumerable<int> Movies { get; set; } = new List<int>();

			public IEnumerable<User> Users { get; set; } = new List<User>();

			public class User
			{
				public int Id { get; set; }
				public string UserName { get; set; }
			}
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
