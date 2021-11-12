using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
	public static class PublicationClassSeedData
	{
		public static readonly PublicationClass[] Classes =
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
				IconPath = "images/moon.png",
				Link = "Moons"
			},
			new ()
			{
				Id = 3,
				Name = "Stars",
				Weight = 1.5,
				IconPath = "images/star.png",
				Link = "Stars"
			}
		};
	}
}
