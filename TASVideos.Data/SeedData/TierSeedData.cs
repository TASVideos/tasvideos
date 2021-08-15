using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
	public static class TierSeedData
	{
		public static readonly Tier[] Tiers =
		{
			new ()
			{
				Id = 1,
				Name = "Standard",
				Weight = 1,
				Link = "Standard"
			},
			new ()
			{
				Id = 2,
				Name = "Moons",
				Weight = 1,
				IconPath = "images/moontier.png",
				Link = "Moons"
			},
			new ()
			{
				Id = 3,
				Name = "Stars",
				Weight = 1.5,
				IconPath = "images/startier.png",
				Link = "Stars"
			}
		};
	}
}
