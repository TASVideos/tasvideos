using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1401
namespace TASVideos.Data.SeedData
{
	public static class TierSeedData
	{
		public static Tier[] Tiers =
		{
			new()
			{
				Id = 1,
				Name = "Vault",
				Weight = 0.75,
				IconPath = "images/vaulttier.png",
				Link = "Vault"
			},
			new()
			{
				Id = 2,
				Name = "Moons",
				Weight = 1,
				IconPath = "images/moontier.png",
				Link = "Moons"
			},
			new()
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
