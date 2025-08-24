using TASVideos.Pages.GameGroups;

namespace TASVideos.RazorPages.Tests.Pages.GameGroups;

[TestClass]
public class ListModelTests : BasePageModelTests
{
	private readonly ListModel _model;

	public ListModelTests()
	{
		_model = new ListModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoGameGroups_LoadsEmptyList()
	{
		await _model.OnGet();
		Assert.AreEqual(0, _model.GameGroups.Count);
	}

	[TestMethod]
	public async Task OnGet_WithMultipleGameGroups_LoadsCorrectData()
	{
		var group1 = _db.AddGameGroup("A Series").Entity;
		var group2 = _db.AddGameGroup("B Series").Entity;
		var group3 = _db.AddGameGroup("C Series").Entity;
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(3, _model.GameGroups.Count);

		var loadedGroup1 = _model.GameGroups.FirstOrDefault(g => g.Id == group1.Id);
		Assert.IsNotNull(loadedGroup1);
		Assert.AreEqual("A Series", loadedGroup1.Name);

		var loadedGroup2 = _model.GameGroups.FirstOrDefault(g => g.Id == group2.Id);
		Assert.IsNotNull(loadedGroup2);
		Assert.AreEqual("B Series", loadedGroup2.Name);

		var loadedGroup3 = _model.GameGroups.FirstOrDefault(g => g.Id == group3.Id);
		Assert.IsNotNull(loadedGroup3);
		Assert.AreEqual("C Series", loadedGroup3.Name);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ListModel));
}
