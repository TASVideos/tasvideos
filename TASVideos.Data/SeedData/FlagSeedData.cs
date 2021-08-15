using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
	public static class FlagSeedData
	{
		public static readonly Flag[] Flags =
		{
			new ()
			{
				Id = 1,
				Name = "Notable improvement",
				IconPath = "images/notable.png",
				LinkPath = "NotableImprovements",
				Token = "Improve",
				PermissionRestriction = PermissionTo.PublishMovies
			},
			new ()
			{
				Id = 2,
				Name = "Console-verified",
				IconPath = "images/verified.png",
				LinkPath = "ConsoleVerifiedMovies",
				Token = "Verified"
			},
			new ()
			{
				Id = 4,
				Name = "First platform",
				Token = "Firstplatform"
			},
			new ()
			{
				Id = 5,
				Name = "Fastest Completion",
				LinkPath = "FastestCompletion",
				Token = "Knownrecords",
				IconPath = "images/fastest-completion.png"
			},
			new ()
			{
				Id = 6,
				Name = "Has commentary",
				IconPath = "images/commentary.png",
				LinkPath = "Commentaries",
				Token = "Commentary"
			},
			new ()
			{
				Id = 7,
				Name = "Atlas Map Encode",
				LinkPath = "Movies-Atlas",
				Token = "Atlas"
			},
			new ()
			{
				Id = 8,
				Name = "Recommended for newcomers",
				IconPath = "images/newbierec.gif",
				LinkPath = "NewcomerCorner",
				Token = "NewcomerRec",
				PermissionRestriction = PermissionTo.EditRecommendation
			}
		};
	}
}
