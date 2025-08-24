using TASVideos.Pages.Account;

namespace TASVideos.RazorPages.Tests.Pages.Account;

[TestClass]
public class SendConfirmationEmailTests : BasePageModelTests
{
	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(SendConfirmationEmail));

	[TestMethod]
	public void HasIpBanCheckAttribute() => AssertHasIpBanCheck(typeof(SendConfirmationEmail));
}
