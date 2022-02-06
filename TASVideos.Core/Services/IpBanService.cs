using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTools;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface IIpBanService
{
	/// <summary>
	/// Returns a value indicating whether or not the ip address is currently banned.
	/// </summary>
	ValueTask<bool> IsBanned(IPAddress? ipAddress);

	/// <summary>
	/// Returns a list of all banned IP Address/Ranges
	/// </summary>
	Task<IEnumerable<IpBanEntry>> GetAll();

	/// <summary>
	/// Adds an IP Address or Address range to the ban list
	/// </summary>
	/// <param name="ipMask">The mask pattern to ban, can be an ipv4, ipv6, ipv4 star notion, or ipv6 CIDR notion</param>
	/// <returns>True if the mask can be parsed and successfully saved to the database, else false</returns>
	Task<bool> Add(string ipMask);

	/// <summary>
	/// Removes the given IP Address or Address Range, and clears the cache if found
	/// </summary>
	Task Remove(string ipMask);
}

internal class IpBanService : IIpBanService
{
	internal const string IpBanList = "IpBanList";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;
	private readonly ILogger<IpBanService> _logger;

	public IpBanService(
		ApplicationDbContext db,
		ICacheService cache,
		ILogger<IpBanService> logger)
	{
		_db = db;
		_cache = cache;
		_logger = logger;
	}

	public async ValueTask<bool> IsBanned(IPAddress? ipAddress)
	{
		if (ipAddress is null)
		{
			return false;
		}

		var bans = await BannedIps();
		return bans.Any(b => b.Contains(ipAddress));
	}

	public async Task<IEnumerable<IpBanEntry>> GetAll()
	{
		return await _db.IpBans
			.Select(b => new IpBanEntry(b.Mask, b.CreateUserName, b.CreateTimestamp))
			.ToListAsync();
	}

	private async ValueTask<IEnumerable<IPAddressRange>> BannedIps()
	{
		if (_cache.TryGetValue(IpBanList, out IEnumerable<IPAddressRange> list))
		{
			return list;
		}

		var rawIps = await _db.IpBans.ToListAsync();
		var parsed = rawIps
			.Select(r => ToAddressRange(r.Mask))
			.Where(i => i is not null)
			.ToList();

		_cache.Set(IpBanList, parsed);
		return parsed!;
	}

	private IPAddressRange? ToAddressRange(string mask)
	{
		try
		{
			string processed = "";
			if (mask.Contains('*'))
			{
				if (mask.EndsWith("*.*.*"))
				{
					processed = mask.Replace("*.*.*", "0.00.0") + "/255.0.0.0";
				}
				else if (mask.EndsWith("*.*"))
				{
					processed = mask.Replace("*.*", "0.0") + "/255.255.0.0";
				}
				else if (mask.EndsWith("*"))
				{
					processed = mask.Replace("*", "0") + "/255.255.255.0";
				}
			}
			else
			{
				processed = mask;
			}

			if (string.IsNullOrEmpty(processed))
			{
				LogError(mask);
				return null;
			}

			return IPAddressRange.Parse(processed);
		}
		catch (FormatException)
		{
			LogError(mask);
			return null;
		}
	}

	public async Task<bool> Add(string ipMask)
	{
		if (string.IsNullOrWhiteSpace(ipMask))
		{
			return false;
		}

		if (ToAddressRange(ipMask) == null)
		{
			return false;
		}

		_db.IpBans.Add(new IpBan { Mask = ipMask });

		try
		{
			await _db.SaveChangesAsync();
		}
		catch
		{
			return false;
		}

		_cache.Remove(IpBanList);
		return true;
	}

	public async Task Remove(string ipMask)
	{
		var entry = await _db.IpBans.SingleOrDefaultAsync(b => b.Mask == ipMask);
		if (entry != null)
		{
			_db.IpBans.Remove(entry);
			await _db.SaveChangesAsync();
			_cache.Remove(IpBanList);
		}
	}

	private void LogError(string mask) => _logger.LogError("Unable to parse ban address mask {mask}", mask);
}

public record IpBanEntry(string Mask, string? CreateUserName, DateTime DateCreated);
