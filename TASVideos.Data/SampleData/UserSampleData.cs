using TASVideos.Data.Entity;

namespace TASVideos.Data.SampleData
{
	// User data is sample data not seed data because we do not want to go to production with known credentials
	public class UserSampleData
	{
		public const string SamplePassword = "Password1234!@#$"; // Obviously no one should use this in production

		public static readonly User[] AdminUsers =
		{
			new User
			{
				UserName = "adelikat",
				NormalizedUserName = "ADELIKAT",
				Email = "adelikat@tasvideos.org",
				TimeZoneId = "Central Standard Time"
			}
		};

		public static readonly User[] Judges =
		{
			new User
			{
				UserName = "Judge",
				NormalizedUserName = "JUDGE",
				Email = "judge@example.com",
				TimeZoneId = "Eastern Standard Time"
			},
			new User
			{
				UserName = "Judge2",
				NormalizedUserName = "JUDGE2",
				Email = "judge2@example.com",
				TimeZoneId = "Eastern Standard Time"
			}
		};

		public static readonly User[] Users =
		{
			new User
			{
				UserName = "Dara.Marks",
				NormalizedUserName = "DARA.MARKS",
				Email = "dara.marks@example.com"
			}
		};
	}
}
