using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1401
namespace TASVideos.Data.SeedData
{
    public static class TierSeedData
	{
		public static Tier[] Tiers =
		{
			new Tier
			{
				Name = "Moons",
				Weight = 1,
				IconPath = "images/moontier.png",
				Link = "Moons"
			},
			new Tier
			{
				Name = "Stars",
				Weight = 1.5,
				IconPath = "images/startier.png",
				Link = "Stars"
			},
			new Tier
			{
				Name = "Vault",
				Weight = 0.75,
				IconPath = "images/vaulttier.png",
				Link = "Vault"
			}
		};
	}
}
