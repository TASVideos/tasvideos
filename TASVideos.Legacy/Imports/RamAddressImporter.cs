using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal static class RamAddressImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var domains = legacySiteContext.RamAddressDomains
				.Select(rd => new GameRamAddressDomain
				{
					Id = rd.Id,
					Name = rd.Name ?? "",
					GameSystemId = rd.SystemId
				})
				.ToList();

			var domainColumns = new[]
			{
				nameof(GameRamAddressDomain.Id),
				nameof(GameRamAddressDomain.Name),
				nameof(GameRamAddressDomain.GameSystemId)
			};

			var legacyRamAddresses = legacySiteContext.RamAddresses
				.Include(r => r.AddressSet)
				.ToList();

			var games = context.Games.ToList();

			var ramAddresses =
				(from address in legacyRamAddresses
				join gg in games
					on new { name = address?.AddressSet?.Name?.ToLower(), systemId = address?.AddressSet?.SystemId }
					equals new { name = gg.DisplayName.ToLower(), systemId = (int?)gg.SystemId } into game
				select new { address, game })
				.Select(l => new GameRamAddress
				{
					Id = l.address.Id,
					LegacySetId = l.address.AddressSetId,
					Address = l.address.Address,
					Type = ToType(l.address.Type),
					Signed = ToSigned(l.address.Signed),
					Endian = ToEndian(l.address.Endian),
					Description = l.address.Description ?? "",
					GameRamAddressDomainId = l.address.Domain,
					GameId = l.game.Any() ? l.game.First().Id : null,
					SystemId = l.address?.AddressSet?.SystemId ?? 0,
					LegacyGameName = !l.game.Any() ? l.address?.AddressSet?.Name : null
				})
				.ToList();

			// Squash sets into the appropriate game
			var flintStones = new[] { 24, 25, 26 };
			foreach (var addr in ramAddresses)
			{
				if (flintStones.Contains(addr.LegacySetId))
				{
					addr.GameId = 1219;
				}
				else if (addr.LegacySetId == 83)
				{
					addr.GameId = 1460;
				}
				else if (addr.LegacySetId == 117)
				{
					addr.GameId = 1481;
				}
			}

			var addressColumns = new[]
			{
				nameof(GameRamAddress.Id),
				nameof(GameRamAddress.LegacySetId),
				nameof(GameRamAddress.Address),
				nameof(GameRamAddress.Type),
				nameof(GameRamAddress.Signed),
				nameof(GameRamAddress.Endian),
				nameof(GameRamAddress.Description),
				nameof(GameRamAddress.GameRamAddressDomainId),
				nameof(GameRamAddress.GameId),
				nameof(GameRamAddress.LegacyGameName),
				nameof(GameRamAddress.SystemId)
			};

			domains.BulkInsert(domainColumns, nameof(ApplicationDbContext.GameRamAddressDomains));
			ramAddresses.BulkInsert(addressColumns, nameof(ApplicationDbContext.GameRamAddresses));
		}

		private static RamAddressType ToType(string? type)
		{
			return type?.ToLower() switch
			{
				"byte" => RamAddressType.Byte,
				"word" => RamAddressType.Word,
				"dword" => RamAddressType.DWord,
				"float" => RamAddressType.Float,
				"q12.4" => RamAddressType.Q12_4,
				"q20.12" => RamAddressType.Q20_12,
				"q20.4" => RamAddressType.Q20_4,
				"q28.4" => RamAddressType.Q28_4,
				"q8.8" => RamAddressType.Q8_8,
				"q16.8" => RamAddressType.Q16_8,
				"q24.8" => RamAddressType.Q24_8,
				"q16.16" => RamAddressType.Q16_16,
				"tbyte" => RamAddressType.ThreeByte,
				_ => throw new NotImplementedException($"Unknown type: {type}"),
			};
		}

		private static RamAddressSigned ToSigned(string? signed)
		{
			return signed?.ToLower() switch
			{
				"signed" => RamAddressSigned.Signed,
				"unsigned" => RamAddressSigned.Unsigned,
				"hex" => RamAddressSigned.Hex,
				_ => throw new NotImplementedException($"Unknown signed: {signed}")
			};
		}

		private static RamAddressEndian ToEndian(string? endian)
		{
			return endian?.ToLower() switch
			{
				"big" => RamAddressEndian.Big,
				"little" => RamAddressEndian.Little,
				"host" => RamAddressEndian.Host,
				_ => throw new NotImplementedException($"Unknown endian: {endian}")
			};
		}
	}
}
