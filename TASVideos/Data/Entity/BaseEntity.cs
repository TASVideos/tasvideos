using System;

namespace TASVideos.Data.Entity
{
    public interface ITrackable
    {
		DateTime CreateTimeStamp { get; set; }
		DateTime LastUpdateTimeStamp { get; set; }
		//TODO: createuser and lastupdateuser
	}
}
