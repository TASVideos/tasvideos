namespace TASVideos.Services.ExternalMediaPublisher
{
    public enum PostType
	{
		/// <summary>
		/// A highly time sensitive administrative alert
		/// Should be used for emergency situations
		/// Should never be sent to the general public
		/// </summary>
		Critical,

		/// <summary>
		/// Administrative only alerts,
		/// Should not be visible to the general public
		/// </summary>
		Administrative,

		/// <summary>
		/// A public announcement to the general public
		/// Should be used for alerts such as new Publications
		/// Should be used for things like tweets
		/// </summary>
		Announcement,

		/// <summary>
		/// General messages that can be seen by the public
		/// Should be used for common activities such as new forum posts or wiki edits
		/// </summary>
		General,

		/// <summary>
		/// Information that is only useful for logging purposes
		/// </summary>
		Log
	}
}
