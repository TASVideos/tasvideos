using System.Security.Claims;

namespace TASVideos.RazorPages.Tests.Extensions;

[TestClass]
public class ClaimsPrincipalExtensionsTests
{
	[TestMethod]
	public void CanEditWiki_NullUser_CallsWikiHelper()
	{
		ClaimsPrincipal? user = null;
		var result = user.CanEditWiki("TestPage");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditWiki_ValidUser_ReturnsTrue()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditWikiPages)));
		var user = new ClaimsPrincipal(identity);

		var result = user.CanEditWiki("RegularWikiPage");

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditWiki_UserWithoutPermission_ReturnsFalse()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		var user = new ClaimsPrincipal(identity);

		var result = user.CanEditWiki("RegularWikiPage");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditWiki_SpecialPageWithCorrectPermission_ReturnsTrue()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditSubmissions)));
		var user = new ClaimsPrincipal(identity);

		var result = user.CanEditWiki(LinkConstants.SubmissionWikiPage + "123");

		Assert.IsTrue(result);
	}
}
