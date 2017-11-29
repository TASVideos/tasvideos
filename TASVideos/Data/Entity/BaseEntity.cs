using System;

namespace TASVideos.Data.Entity
{
    public interface ITrackable
    {
		DateTime CreateTimeStamp { get; set; }
		string CreateUserName { get; set; }

		DateTime LastUpdateTimeStamp { get; set; }
		string LastUpdateUserName { get; set; }
	}

	public class BaseEntity : ITrackable
	{
		public DateTime CreateTimeStamp { get; set; }
		public string CreateUserName { get; set; }

		public DateTime LastUpdateTimeStamp { get; set; }
		public string LastUpdateUserName { get; set; }
	}
}
