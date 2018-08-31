using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
    public static class FlagSeedData
    {
		public static readonly Flag[] Flags =
		{
			new Flag
			{
				Id = 1,
				Name = "Notable improvement",
				IconPath = "images/notable.png",
				LinkPath = "NotableImprovements",
				Token = "Improve",
				PermissionRestriction = PermissionTo.PublishMovies
			},
			new Flag
			{
				Id = 2,
				Name = "Console-verified",
				IconPath = "images/verified.png",
				LinkPath = "ConsoleVerifiedMovies",
				Token = "Verified"
			},
			new Flag
			{
				Id = 4,
				Name = "First platform",
				Token = "Firstplatform"
			},
			new Flag
			{
				Id = 5,
				Name = "Known record",
				LinkPath = "Movies-Knownrecords",
				Token = "Knownrecords"
			},
			new Flag
			{
				Id = 6,
				Name = "Has commentary",
				IconPath = "images/commentary.png",
				LinkPath = "Commentaries",
				Token = "Commentaries"
			},
			new Flag
			{
				Id = 7,
				Name = "Atlas Map Encode",
				LinkPath = "Movies-Atlas",
				Token = "Atlas"
			},
			new Flag
			{
				Id = 8,
				Name = "Recommended for newcomers",
				IconPath = "images/newbierec.gif",
				LinkPath = "NewcomerCorner",
				Token = "NewcomerRec",
				PermissionRestriction = PermissionTo.EditRecommendation
			},
			new Flag
			{
				Id = 9,
				Name = "Fastest Completion",
				IconPath = "images/fastest-completion.png",
				LinkPath = "FastestCompletion",
				Token = "FastestCompletion",
				PermissionRestriction = PermissionTo.EditRecommendation
			}
		};
    }
}
