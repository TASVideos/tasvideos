using TASVideos.Data.Entity;
using TASVideos.Tests.Base;

namespace TASVideos.Data.Tests.Context;

[TestClass]
public class ApplicationDbContextTests : TestDbBase
{
	[TestMethod]
	public async Task CreateTimestamp_SetToNow_IfNotProvided()
	{
		_db.AddPublication();

		await _db.SaveChangesAsync();

		Assert.AreEqual(1, _db.Publications.Count());
		var pub = _db.Publications.Single();
		Assert.IsTrue(pub.CreateTimestamp.Year > 1);
	}

	[TestMethod]
	public async Task CreateTimestamp_DoesNotOverride_IfProvided()
	{
		var pubEntity = _db.AddPublication().Entity;
		pubEntity.CreateTimestamp = DateTime.Parse("01/01/1970");

		await _db.SaveChangesAsync();

		Assert.AreEqual(1, _db.Publications.Count());

		var pub = _db.Publications.Single();
		Assert.AreEqual(1970, pub.CreateTimestamp.Year);
	}

	[TestMethod]
	public async Task CreateTimestamp_OnUpdate_DoesNotChange()
	{
		var pub = _db.AddPublication().Entity;
		pub.CreateTimestamp = DateTime.Parse("01/01/1970");

		await _db.SaveChangesAsync();

		pub.Title = "NewTitle";

		await _db.SaveChangesAsync();

		Assert.AreEqual(1970, pub.CreateTimestamp.Year);
	}

	[TestMethod]
	public async Task LastUpdateTimestamp_OnCreate_SetToNow_IfNotProvided()
	{
		_db.AddPublication();

		await _db.SaveChangesAsync();

		Assert.AreEqual(1, _db.Publications.Count());
		var pub = _db.Publications.Single();
		Assert.IsTrue(pub.LastUpdateTimestamp.Year > 1);
	}

	[TestMethod]
	public async Task LastUpdateTimestamp_OnCreate_DoesNotOverride_IfProvided()
	{
		_db.IpBans.Add(new IpBan
		{
			LastUpdateTimestamp = DateTime.Parse("01/01/1970")
		});

		await _db.SaveChangesAsync();

		Assert.AreEqual(1, _db.IpBans.Count());
		var ban = _db.IpBans.Single();
		Assert.AreEqual(1970, ban.LastUpdateTimestamp.Year);
	}

	[TestMethod]
	public async Task LastUpdateTimestamp_OnUpdate_SetToNow_IfNotProvided()
	{
		var pubEntity = _db.AddPublication().Entity;
		pubEntity.CreateTimestamp = DateTime.Parse("01/01/1970");

		await _db.SaveChangesAsync();

		var pub = _db.Publications.Single();
		pub.Title = "NewTitle";

		await _db.SaveChangesAsync();

		Assert.AreEqual(DateTime.UtcNow.Year, pub.LastUpdateTimestamp.Year);
	}

	[TestMethod]
	public async Task LastUpdateTimestamp_OnUpdate_DoesNotOverride_IfProvided()
	{
		var pubEntity = _db.AddPublication().Entity;
		pubEntity.CreateTimestamp = DateTime.Parse("01/01/1970");

		await _db.SaveChangesAsync();

		var pub = _db.Publications.Single();
		pub.LastUpdateTimestamp = DateTime.Parse("01/01/1980");

		await _db.SaveChangesAsync();

		Assert.AreEqual(1980, pub.LastUpdateTimestamp.Year);
	}
}
