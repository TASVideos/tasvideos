using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class PublicationSearchModelTests
{
	[TestMethod]
	public void Empty()
	{
		var model = new PublicationSearchModel
		{
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void Class()
	{
		var model = new PublicationSearchModel
		{
			Classes = new[] { "Standard" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard", actual);
	}

	[TestMethod]
	public void Classes()
	{
		var model = new PublicationSearchModel
		{
			Classes = new[] { "Standard", "Stars" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars", actual);
	}

	[TestMethod]
	public void SystemCode()
	{
		var model = new PublicationSearchModel
		{
			SystemCodes = new[] { "NES" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES", actual);
	}

	[TestMethod]
	public void Systems()
	{
		var model = new PublicationSearchModel
		{
			SystemCodes = new[] { "NES", "N64" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64", actual);
	}

	[TestMethod]
	public void ClassesAndSystems()
	{
		var model = new PublicationSearchModel
		{
			Classes = new[] { "Standard", "Stars" },
			SystemCodes = new[] { "NES", "N64" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars-NES-N64", actual);
	}

	[TestMethod]
	public void Year()
	{
		var model = new PublicationSearchModel
		{
			Years = new[] { 2000 }
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Y2000", actual);
	}

	[TestMethod]
	public void Years()
	{
		var model = new PublicationSearchModel
		{
			Years = new[] { 2000, 2001 }
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Y2000-Y2001", actual);
	}

	[TestMethod]
	public void YearsAndClasses()
	{
		var model = new PublicationSearchModel
		{
			Classes = new[] { "Standard", "Stars" },
			Years = new[] { 2000, 2001 }
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars-Y2000-Y2001", actual);
	}

	[TestMethod]
	public void YearsAndSystemsAndClasses()
	{
		var model = new PublicationSearchModel
		{
			Classes = new[] { "Standard", "Stars" },
			SystemCodes = new[] { "NES", "N64" },
			Years = new[] { 2000, 2001 }
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Standard-Stars-NES-N64-Y2000-Y2001", actual);
	}

	[TestMethod]
	public void Tag()
	{
		var model = new PublicationSearchModel
		{
			Tags = new[] { "1p" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1p", actual);
	}

	[TestMethod]
	public void Tags()
	{
		var model = new PublicationSearchModel
		{
			Tags = new[] { "1p", "2p" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1p-2p", actual);
	}

	[TestMethod]
	public void TagsAndYears()
	{
		var model = new PublicationSearchModel
		{
			Tags = new[] { "1p", "2p" },
			Years = new[] { 2000, 2001 }
		};

		var actual = model.ToUrl();
		Assert.AreEqual("Y2000-Y2001-1p-2p", actual);
	}

	[TestMethod]
	public void TagsAndSystemsAndYears()
	{
		var model = new PublicationSearchModel
		{
			Tags = new[] { "1p", "2p" },
			SystemCodes = new[] { "NES", "N64" },
			Years = new[] { 2000, 2001 }
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-Y2000-Y2001-1p-2p", actual);
	}

	[TestMethod]
	public void Genres()
	{
		var model = new PublicationSearchModel
		{
			Genres = new[] { "action", "adventure" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("action-adventure", actual);
	}

	[TestMethod]
	public void TagsAndGenres()
	{
		var model = new PublicationSearchModel
		{
			Tags = new[] { "1p", "2p" },
			Genres = new[] { "action", "adventure" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1p-2p-action-adventure", actual);
	}

	[TestMethod]
	public void Flags()
	{
		var model = new PublicationSearchModel
		{
			Flags = new[] { "atlas", "verified" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("atlas-verified", actual);
	}

	[TestMethod]
	public void SystemsAndFlags()
	{
		var model = new PublicationSearchModel
		{
			Flags = new[] { "atlas", "verified" },
			SystemCodes = new[] { "NES", "N64" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-atlas-verified", actual);
	}

	[TestMethod]
	public void SystemsAndTagsAndGenres()
	{
		var model = new PublicationSearchModel
		{
			SystemCodes = new[] { "NES", "N64" },
			Tags = new[] { "1p", "2p" },
			Genres = new[] { "action", "adventure" },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-1p-2p-action-adventure", actual);
	}

	[TestMethod]
	public void Games()
	{
		var model = new PublicationSearchModel
		{
			Games = new[] { 1, 2 },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("1g-2g", actual);
	}

	[TestMethod]
	public void GenresAndGame()
	{
		var model = new PublicationSearchModel
		{
			Genres = new[] { "action", "adventure" },
			Games = new[] { 1, 2 },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("action-adventure-1g-2g", actual);
	}

	[TestMethod]
	public void GameGroups()
	{
		var model = new PublicationSearchModel
		{
			GameGroups = new[] { 1, 2 },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("group1-group2", actual);
	}

	[TestMethod]
	public void GamesAndGameGroups()
	{
		var model = new PublicationSearchModel
		{
			Games = new[] { 3, 4 },
			GameGroups = new[] { 1, 2 },
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("3g-4g-group1-group2", actual);
	}

	[TestMethod]
	public void ShowObsolete_Empty()
	{
		var model = new PublicationSearchModel
		{
			ShowObsoleted = true,
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void SystemAndObsolete()
	{
		var model = new PublicationSearchModel
		{
			SystemCodes = new[] { "NES", "N64" },
			ShowObsoleted = true,
			Years = Array.Empty<int>()
		};

		var actual = model.ToUrl();
		Assert.AreEqual("NES-N64-Obs", actual);
	}
}
