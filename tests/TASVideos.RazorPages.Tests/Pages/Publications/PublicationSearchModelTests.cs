using static TASVideos.Pages.Publications.IndexModel;
namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class PublicationSearchModelTests
{
	[TestMethod]
	public void Empty()
	{
		var model = new PublicationSearch
		{
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void Class()
	{
		var model = new PublicationSearch
		{
			Classes = ["Standard"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard", actual);
	}

	[TestMethod]
	public void Classes()
	{
		var model = new PublicationSearch
		{
			Classes = ["Standard", "Stars"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars", actual);
	}

	[TestMethod]
	public void SystemCode()
	{
		var model = new PublicationSearch
		{
			SystemCodes = ["NES"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES", actual);
	}

	[TestMethod]
	public void Systems()
	{
		var model = new PublicationSearch
		{
			SystemCodes = ["NES", "N64"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64", actual);
	}

	[TestMethod]
	public void ClassesAndSystems()
	{
		var model = new PublicationSearch
		{
			Classes = ["Standard", "Stars"],
			SystemCodes = ["NES", "N64"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars-NES-N64", actual);
	}

	[TestMethod]
	public void Year()
	{
		var model = new PublicationSearch
		{
			Years = [2000]
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Y2000", actual);
	}

	[TestMethod]
	public void Years()
	{
		var model = new PublicationSearch
		{
			Years = [2000, 2001]
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Y2000-Y2001", actual);
	}

	[TestMethod]
	public void YearsAndClasses()
	{
		var model = new PublicationSearch
		{
			Classes = ["Standard", "Stars"],
			Years = [2000, 2001]
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars-Y2000-Y2001", actual);
	}

	[TestMethod]
	public void YearsAndSystemsAndClasses()
	{
		var model = new PublicationSearch
		{
			Classes = ["Standard", "Stars"],
			SystemCodes = ["NES", "N64"],
			Years = [2000, 2001]
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars-NES-N64-Y2000-Y2001", actual);
	}

	[TestMethod]
	public void Tag()
	{
		var model = new PublicationSearch
		{
			Tags = ["1p"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1p", actual);
	}

	[TestMethod]
	public void Tags()
	{
		var model = new PublicationSearch
		{
			Tags = ["1p", "2p"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1p-2p", actual);
	}

	[TestMethod]
	public void TagsAndYears()
	{
		var model = new PublicationSearch
		{
			Tags = ["1p", "2p"],
			Years = [2000, 2001]
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Y2000-Y2001-1p-2p", actual);
	}

	[TestMethod]
	public void TagsAndSystemsAndYears()
	{
		var model = new PublicationSearch
		{
			Tags = ["1p", "2p"],
			SystemCodes = ["NES", "N64"],
			Years = [2000, 2001]
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-Y2000-Y2001-1p-2p", actual);
	}

	[TestMethod]
	public void Genres()
	{
		var model = new PublicationSearch
		{
			Genres = ["action", "adventure"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("action-adventure", actual);
	}

	[TestMethod]
	public void TagsAndGenres()
	{
		var model = new PublicationSearch
		{
			Tags = ["1p", "2p"],
			Genres = ["action", "adventure"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1p-2p-action-adventure", actual);
	}

	[TestMethod]
	public void Flags()
	{
		var model = new PublicationSearch
		{
			Flags = ["atlas", "verified"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("atlas-verified", actual);
	}

	[TestMethod]
	public void SystemsAndFlags()
	{
		var model = new PublicationSearch
		{
			Flags = ["atlas", "verified"],
			SystemCodes = ["NES", "N64"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-atlas-verified", actual);
	}

	[TestMethod]
	public void SystemsAndTagsAndGenres()
	{
		var model = new PublicationSearch
		{
			SystemCodes = ["NES", "N64"],
			Tags = ["1p", "2p"],
			Genres = ["action", "adventure"],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-1p-2p-action-adventure", actual);
	}

	[TestMethod]
	public void Games()
	{
		var model = new PublicationSearch
		{
			Games = [1, 2],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1g-2g", actual);
	}

	[TestMethod]
	public void GenresAndGame()
	{
		var model = new PublicationSearch
		{
			Genres = ["action", "adventure"],
			Games = [1, 2],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("action-adventure-1g-2g", actual);
	}

	[TestMethod]
	public void GameGroups()
	{
		var model = new PublicationSearch
		{
			GameGroups = [1, 2],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("group1-group2", actual);
	}

	[TestMethod]
	public void GamesAndGameGroups()
	{
		var model = new PublicationSearch
		{
			Games = [3, 4],
			GameGroups = [1, 2],
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("3g-4g-group1-group2", actual);
	}

	[TestMethod]
	public void ShowObsolete_Empty()
	{
		var model = new PublicationSearch
		{
			ShowObsoleted = true,
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void SystemAndObsolete()
	{
		var model = new PublicationSearch
		{
			SystemCodes = ["NES", "N64"],
			ShowObsoleted = true,
			Years = []
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-Obs", actual);
	}
}
