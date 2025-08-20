using TASVideos.Pages.Forum.Legacy;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Legacy;

[TestClass]
public class MoodReportModelTests : BasePageModelTests
{
	private readonly MoodReportModel _model = new()
	{
		PageContext = TestPageContext()
	};

	[TestMethod]
	public void OnGet_NoReturnUrl_RedirectsToMoodReportPage()
	{
		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/MoodReport", redirectResult.PageName);
		Assert.IsNull(redirectResult.RouteValues);
	}
}
