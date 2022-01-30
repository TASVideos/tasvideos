using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class IpBanServiceTests
{
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly IpBanService _banService;

	public IpBanServiceTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_banService = new IpBanService(_db, _cache, new NullLogger<IpBanService>());
	}

	[TestMethod]
	public async Task IsBanned_Null_ReturnsFalse()
	{
		var result = await _banService.IsBanned(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	[DataRow("192.168.0.1", "192.168.0.1")]
	[DataRow("192.168.0.*", "192.168.0.1")]
	[DataRow("192.168.*.*", "192.168.0.1")]
	[DataRow("192.168.*.*", "192.168.128.0")]
	[DataRow("192.*.*.*", "192.168.0.1")]
	[DataRow("192.*.*.*", "192.168.10.1")]
	[DataRow("192.*.*.*", "192.128.10.1")]
	public async Task IsBanned_ValidMask_ReturnsTrue(string mask, string ip)
	{
		_db.IpBans.Add(new IpBan { Mask = mask });
		await _db.SaveChangesAsync();
		var result = await _banService.IsBanned(IPAddress.Parse(ip));
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("\n")]
	[DataRow("UnknownFormat")]
	public async Task Add_Invalid_ReturnsFalse(string ipMask)
	{
		var actual = await _banService.Add(ipMask);
		Assert.IsFalse(actual);
		Assert.AreEqual(0, _db.IpBans.Count());
	}

	[TestMethod]
	[DataRow("192.168.0.1")]
	[DataRow("192.168.0.*")]
	[DataRow("192.168.*.*")]
	[DataRow("192.*.*.*")]
	public async Task Add_SupportedFormat_AddsToDbAndClearsCache(string ipMask)
	{
		_cache.Set(IpBanService.IpBanList, new object());

		var actual = await _banService.Add(ipMask);
		Assert.IsTrue(actual);
		Assert.AreEqual(0, _cache.Count());
		Assert.AreEqual(1, _db.IpBans.Count());
	}

	[TestMethod]
	public async Task Remove_NotFound_DoesNothing()
	{
		const string ipAddress = "192.168.0.1";
		_cache.Set(IpBanService.IpBanList, new object());

		await _banService.Remove(ipAddress);
		Assert.AreEqual(1, _cache.Count());
		Assert.AreEqual(0, _db.IpBans.Count());
	}

	[TestMethod]
	public async Task Remove_Found_RemovesAndClearsCache()
	{
		const string ipAddress = "192.168.0.1";
		_cache.Set(IpBanService.IpBanList, new object());
		_db.IpBans.Add(new IpBan { Mask = ipAddress });
		await _db.SaveChangesAsync();

		await _banService.Remove(ipAddress);
		Assert.AreEqual(0, _cache.Count());
		Assert.AreEqual(0, _db.IpBans.Count());
	}

	[TestMethod]
	public async Task GetAll_NoEntries_ReturnsEmptyList()
	{
		var actual = await _banService.GetAll();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task GetAll_ReturnsData()
	{
		_db.IpBans.Add(new IpBan { Mask = "asdf" });
		await _db.SaveChangesAsync();

		var result = await _banService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count());
	}
}
