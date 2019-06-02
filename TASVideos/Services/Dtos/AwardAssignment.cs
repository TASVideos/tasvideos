using System.Collections.Generic;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Services
{
	/// <summary>
	/// Represents the assignment of an award to a user or movie
	/// Ex: 2010 TASer of the Year
	/// </summary>
	public class AwardAssignment
	{
		public string ShortName { get; set; }
		public string Description { get; set; }
		public int Year { get; set; }
		public AwardType Type { get; set; }
		public IEnumerable<PublicationDto> Publications { get; set; } = new HashSet<PublicationDto>();
		public IEnumerable<UserDto> Users { get; set; } = new HashSet<UserDto>();

		public class UserDto
		{
			public int Id { get; set; }
			public string UserName { get; set; }
		}

		public class PublicationDto
		{
			public int Id { get; set; }
			public string Title { get; set; }
		}
	}

	/// <summary>
	/// Represents a short summary of an award assignment
	/// </summary>
	public class AwardAssignmentSummary
	{
		public string ShortName { get; set; }
		public string Description { get; set; }
		public int Year { get; set; }
	}
}
