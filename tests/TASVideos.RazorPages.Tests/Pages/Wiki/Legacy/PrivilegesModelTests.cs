using TASVideos.Pages.Wiki.Legacy;

namespace TASVideos.RazorPages.Tests.Pages.Wiki.Legacy;

[TestClass]
public class PrivilegesModelTests : BasePageModelTests
{
	private readonly PrivilegesModel _model = new()
	{
		PageContext = TestPageContext()
	};

	[TestMethod]
	public void OnGet_ReturnsRedirectToPermissionsIndex()
	{
		var result = _model.OnGet();
		AssertRedirect(result, "/Permissions/Index");
	}
}
