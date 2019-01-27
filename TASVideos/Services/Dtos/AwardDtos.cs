using System.Collections.Generic;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Services
{
	public class AwardDto
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

	public class AwardEntryDto
	{
		public string ShortName { get; set; }
		public string Description { get; set; }
		public int Year { get; set; }
	}
}
