using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class RoleServiceTests : TestDbBase
{
	private readonly RoleService _roleService;

	public RoleServiceTests()
	{
		_roleService = new RoleService(_db);
	}

	[TestMethod]
	public async Task IsInUse_RoleNotFound_ReturnsFalse()
	{
		var actual = await _roleService.IsInUse(int.MaxValue);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsInUse_RoleExistsWithNoUsersAssigned_ReturnsFalse()
	{
		const int existingRoleId = 1;
		_db.Roles.Add(new Role { Id = existingRoleId, Name = "TestRole" });
		await _db.SaveChangesAsync();

		var actual = await _roleService.IsInUse(existingRoleId);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsInUse_RoleExistsWithOneUserAssigned_ReturnsTrue()
	{
		var user = _db.AddUser(1);
		const int existingRoleId = 1;
		var role = new Role { Id = existingRoleId, Name = "TestRole" };
		_db.Roles.Add(role);
		_db.UserRoles.Add(new UserRole { User = user.Entity, Role = role });
		await _db.SaveChangesAsync();

		var actual = await _roleService.IsInUse(existingRoleId);
		Assert.IsTrue(actual);
	}
}
