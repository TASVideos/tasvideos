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
		public IEnumerable<PublicationModel> Publications { get; set; } = new HashSet<PublicationModel>();
		public IEnumerable<UserModel> Users { get; set; } = new HashSet<UserModel>();

		public class UserModel
		{
			public int Id { get; set; }
			public string UserName { get; set; }
		}

		public class PublicationModel
		{
			public int Id { get; set; }
			public string Title { get; set; }
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
