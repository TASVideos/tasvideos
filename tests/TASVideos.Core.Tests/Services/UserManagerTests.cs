using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public sealed class UserManagerTests : IDisposable
{
	private readonly UserManager _userManager;
	private readonly TestDbContext _db;

	public UserManagerTests()
	{
		_db = TestDbContext.Create();
		_userManager = new UserManager(
			_db,
			new TestCache(),
			Substitute.For<IPointsService>(),
			Substitute.For<ITASVideoAgent>(),
			Substitute.For<IWikiPages>(),
			Substitute.For<IUserStore<User>>(),
			Substitute.For<IOptions<IdentityOptions>>(),
			Substitute.For<IPasswordHasher<User>>(),
			Substitute.For<IEnumerable<IUserValidator<User>>>(),
			Substitute.For<IEnumerable<IPasswordValidator<User>>>(),
			Substitute.For<ILookupNormalizer>(),
			new IdentityErrorDescriber(),
			Substitute.For<IServiceProvider>(),
			Substitute.For<ILogger<UserManager<User>>>());
	}

	[TestMethod]
	[DataRow("test", "test", true)]
	[DataRow("test", "doesNotExist", false)]
	public async Task Exists(string userToAdd, string userToLookup, bool expected)
	{
		_db.AddUser(userToAdd);
		await _db.SaveChangesAsync();

		var actual = await _userManager.Exists(userToLookup);
		Assert.AreEqual(expected, actual);
	}

	public void Dispose()
	{
		_userManager.Dispose();
	}
}
