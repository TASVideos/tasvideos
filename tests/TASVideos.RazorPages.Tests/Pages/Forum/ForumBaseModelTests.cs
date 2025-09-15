using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum;

namespace TASVideos.RazorPages.Tests.Pages.Forum;

[TestClass]
public class ForumBaseModelTests : BasePageModelTests
{
	private readonly TestableForumBaseModel _model = new()
	{
		PageContext = TestPageContext()
	};

	[TestMethod]
	public void TopicTypes_ContainsAllExpectedValues()
	{
		var topicTypes = _model.TopicTypes;

		foreach (var topicType in Enum.GetValues<ForumTopicType>())
		{
			Assert.IsTrue(topicTypes.Any(t => t.Value == ((int)topicType).ToString()));
		}
	}

	[TestMethod]
	public void Moods_ContainsAllExpectedValues()
	{
		var moods = _model.Moods;

		foreach (var moodType in Enum.GetValues<ForumPostMood>())
		{
			Assert.IsTrue(moods.Any(m => m.Value == ((int)moodType).ToString()));
		}
	}

	[TestMethod]
	public void NotFound_ReturnsRedirectToForumNotFoundPage()
	{
		var result = _model.NotFound();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		Assert.AreEqual("/Forum/NotFound", result.PageName);
	}

	// Helper class to test the protected members of BaseForumModel
	private class TestableForumBaseModel : BaseForumModel;
}
