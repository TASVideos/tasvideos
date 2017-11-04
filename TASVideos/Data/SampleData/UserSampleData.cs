using TASVideos.Data.Entity;

namespace TASVideos.Data.SampleData
{
	// User data is sample data not seed data because we do not want to go to production with known credentials
	public class UserSampleData
	{
		public static User[] Users =
		{
			new User
			{
				UserName = "adelikat",
				NormalizedUserName = "ADELIKAT",
				Email = "adelikat@tasvideos.org"
			}
		};

		public const string SamplePassword = "Password1234!@#$"; // Obviously no one should use this in production
	}
}
