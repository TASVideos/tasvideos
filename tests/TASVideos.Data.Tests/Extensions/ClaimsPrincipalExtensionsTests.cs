using System.Security.Claims;
using TASVideos.Data.Entity;

namespace TASVideos.Data.Tests.Extensions;

[TestClass]
public class ClaimsPrincipalExtensionsTests
{
	#region IsLoggedIn

	[TestMethod]
	public void IsLoggedIn_NullUser_ReturnsFalse()
	{
		ClaimsPrincipal? user = null;
		var result = user.IsLoggedIn();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLoggedIn_NullIdentity_ReturnsFalse()
	{
		var user = new ClaimsPrincipal();
		var result = user.IsLoggedIn();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLoggedIn_NotAuthenticated_ReturnsFalse()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var result = user.IsLoggedIn();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLoggedIn_Authenticated_ReturnsTrue()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity("test"));
		var result = user.IsLoggedIn();
		Assert.IsTrue(result);
	}

	#endregion

	#region Name

	[TestMethod]
	public void Name_NullUser_ReturnsEmptyString()
	{
		ClaimsPrincipal? user = null;
		var result = user.Name();
		Assert.AreEqual("", result);
	}

	[TestMethod]
	public void Name_NullIdentity_ReturnsEmptyString()
	{
		var user = new ClaimsPrincipal();
		var result = user.Name();
		Assert.AreEqual("", result);
	}

	[TestMethod]
	public void Name_NullName_ReturnsEmptyString()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var result = user.Name();
		Assert.AreEqual("", result);
	}

	[TestMethod]
	public void Name_ValidName_ReturnsName()
	{
		const string expectedName = "TestUser";
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.Name, expectedName));
		var user = new ClaimsPrincipal(identity);

		var result = user.Name();

		Assert.AreEqual(expectedName, result);
	}

	#endregion

	#region GetUserId

	[TestMethod]
	public void GetUserId_NullUser_ReturnsNegativeOne()
	{
		ClaimsPrincipal? user = null;
		var result = user.GetUserId();
		Assert.AreEqual(-1, result);
	}

	[TestMethod]
	public void GetUserId_NotLoggedIn_ReturnsNegativeOne()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var result = user.GetUserId();
		Assert.AreEqual(-1, result);
	}

	[TestMethod]
	public void GetUserId_MissingNameIdentifierClaim_ReturnsNegativeOne()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity("test"));
		var result = user.GetUserId();
		Assert.AreEqual(-1, result);
	}

	[TestMethod]
	public void GetUserId_ValidUserId_ReturnsUserId()
	{
		const int expectedUserId = 123;
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()));
		var user = new ClaimsPrincipal(identity);

		var result = user.GetUserId();

		Assert.AreEqual(expectedUserId, result);
	}

	[TestMethod]
	public void GetUserId_InvalidUserIdFormat_ThrowsException()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "not-a-number"));
		var user = new ClaimsPrincipal(identity);
		Assert.ThrowsExactly<FormatException>(() => user.GetUserId());
	}

	#endregion

	#region Permissions

	[TestMethod]
	public void Permissions_NullUser_ReturnsEmptyCollection()
	{
		ClaimsPrincipal? user = null;
		var result = user.Permissions();
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void Permissions_NotLoggedIn_ReturnsEmptyCollection()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var result = user.Permissions();
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void Permissions_NoPermissionClaims_ReturnsEmptyCollection()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		var user = new ClaimsPrincipal(identity);

		var result = user.Permissions();

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void Permissions_WithPermissionClaims_ReturnsPermissions()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)));
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditHomePage)));
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		var user = new ClaimsPrincipal(identity);

		var result = user.Permissions();

		Assert.AreEqual(2, result.Count);
		Assert.IsTrue(result.Contains(PermissionTo.CreateForumPosts));
		Assert.IsTrue(result.Contains(PermissionTo.EditHomePage));
	}

	#endregion

	#region Has

	[TestMethod]
	public void Has_NullUser_ReturnsFalse()
	{
		ClaimsPrincipal? user = null;
		var result = user.Has(PermissionTo.CreateForumPosts);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Has_NotLoggedIn_ReturnsFalse()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var result = user.Has(PermissionTo.CreateForumPosts);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Has_DoesNotHavePermission_ReturnsFalse()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditHomePage)));
		var user = new ClaimsPrincipal(identity);

		var result = user.Has(PermissionTo.CreateForumPosts);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Has_HasPermission_ReturnsTrue()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)));
		var user = new ClaimsPrincipal(identity);

		var result = user.Has(PermissionTo.CreateForumPosts);

		Assert.IsTrue(result);
	}

	#endregion

	#region HasAny

	[TestMethod]
	public void HasAny_NullUser_ReturnsFalse()
	{
		ClaimsPrincipal? user = null;
		var permissions = new[] { PermissionTo.CreateForumPosts, PermissionTo.EditHomePage };

		var actual = user.HasAny(permissions);

		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void HasAny_NotLoggedIn_ReturnsFalse()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var permissions = new[] { PermissionTo.CreateForumPosts, PermissionTo.EditHomePage };

		var result = user.HasAny(permissions);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasAny_EmptyPermissionsList_ReturnsFalse()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)));
		var user = new ClaimsPrincipal(identity);

		var result = user.HasAny([]);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasAny_DoesNotHaveAnyPermissions_ReturnsFalse()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.SubmitMovies)));
		var user = new ClaimsPrincipal(identity);
		var permissions = new[] { PermissionTo.CreateForumPosts, PermissionTo.EditHomePage };

		var result = user.HasAny(permissions);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HasAny_HasOneOfThePermissions_ReturnsTrue()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)));
		var user = new ClaimsPrincipal(identity);

		var permissions = new[] { PermissionTo.CreateForumPosts, PermissionTo.EditHomePage };
		var result = user.HasAny(permissions);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void HasAny_HasAllOfThePermissions_ReturnsTrue()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)));
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditHomePage)));
		var user = new ClaimsPrincipal(identity);
		var permissions = new[] { PermissionTo.CreateForumPosts, PermissionTo.EditHomePage };

		var result = user.HasAny(permissions);

		Assert.IsTrue(result);
	}

	#endregion

	#region ReplacePermissionClaims

	[TestMethod]
	public void ReplacePermissionClaims_NullUser_DoesNothing()
	{
		ClaimsPrincipal? user = null;
		var newPermissions = new[] { new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)) };

		// Should not throw
		user.ReplacePermissionClaims(newPermissions);
	}

	[TestMethod]
	public void ReplacePermissionClaims_NotLoggedIn_DoesNothing()
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity());
		var newPermissions = new[] { new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)) };

		user.ReplacePermissionClaims(newPermissions);

		Assert.AreEqual(0, user.Permissions().Count);
	}

	[TestMethod]
	public void ReplacePermissionClaims_ReplacesExistingPermissions()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditHomePage)));
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.SubmitMovies)));
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		var user = new ClaimsPrincipal(identity);

		var newPermissions = new[] { new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.CreateForumPosts)) };
		user.ReplacePermissionClaims(newPermissions);

		var result = user.Permissions();
		Assert.AreEqual(1, result.Count);
		Assert.IsTrue(result.Contains(PermissionTo.CreateForumPosts));

		// Non-permission claims should remain
		Assert.AreEqual("TestUser", user.Name());
	}

	[TestMethod]
	public void ReplacePermissionClaims_EmptyPermissions_RemovesAllPermissions()
	{
		var identity = new ClaimsIdentity("test");
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.EditHomePage)));
		identity.AddClaim(new Claim(CustomClaimTypes.Permission, nameof(PermissionTo.SubmitMovies)));
		identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
		var user = new ClaimsPrincipal(identity);

		user.ReplacePermissionClaims([]);

		var result = user.Permissions();
		Assert.AreEqual(0, result.Count);

		// Non-permission claims should remain
		Assert.AreEqual("TestUser", user.Name());
	}

	#endregion
}
