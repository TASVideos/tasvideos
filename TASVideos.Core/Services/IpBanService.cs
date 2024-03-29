using System.Net;
using Microsoft.Extensions.Logging;
using NetTools;

namespace TASVideos.Core.Services;

public interface IIpBanService
{
	/// <summary>
	/// Returns a value indicating whether the ip address is currently banned.
	/// </summary>
	ValueTask<bool> IsBanned(IPAddress? ipAddress);

	/// <summary>
	/// Returns a list of all banned IP Address/Ranges
	/// </summary>
	Task<ICollection<IpBanEntry>> GetAll();

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

internal class IpBanService(
	ApplicationDbContext db,
	ICacheService cache,
	ILogger<IpBanService> logger)
	: IIpBanService
{
	internal const string IpBanList = "IpBanList";

	public async ValueTask<bool> IsBanned(IPAddress? ipAddress)
	{
		if (ipAddress is null)
		{
			return false;
		}

		var bans = await BannedIps();
		return bans.Any(b => b.Contains(ipAddress));
	}

	public async Task<ICollection<IpBanEntry>> GetAll()
	{
		return await db.IpBans
			.Select(b => new IpBanEntry(b.Mask, b.CreateTimestamp))
			.ToListAsync();
	}

	private async ValueTask<IEnumerable<IPAddressRange>> BannedIps()
	{
		if (cache.TryGetValue(IpBanList, out List<IpBan> list))
		{
			return list.Select(r => ToAddressRange(r.Mask))
				.Where(i => i is not null)
				.ToList()!;
		}

		var rawIps = await db.IpBans.ToListAsync();
		cache.Set(IpBanList, rawIps);

		var parsed = rawIps
			.Select(r => ToAddressRange(r.Mask))
			.Where(i => i is not null)
			.ToList();

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
				else if (mask.EndsWith('*'))
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

		if (ToAddressRange(ipMask) is null)
		{
			return false;
		}

		db.IpBans.Add(new IpBan { Mask = ipMask });

		try
		{
			await db.SaveChangesAsync();
		}
		catch
		{
			return false;
		}

		cache.Remove(IpBanList);
		return true;
	}

	public async Task Remove(string ipMask)
	{
		var entry = await db.IpBans.SingleOrDefaultAsync(b => b.Mask == ipMask);
		if (entry != null)
		{
			db.IpBans.Remove(entry);
			await db.SaveChangesAsync();
			cache.Remove(IpBanList);
		}
	}

	private void LogError(string mask) => logger.LogError("Unable to parse ban address mask {mask}", mask);
}

public record IpBanEntry(string Mask, DateTime DateCreated);
