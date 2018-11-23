using System;
using System.Linq;

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

	public static class TrackableQueryableExtensions
	{
		public static IQueryable<ITrackable> CreatedBy(this IQueryable<ITrackable> list, string userName)
		{
			return list.Where(t => t.CreateUserName == userName);
		}
	}
}
