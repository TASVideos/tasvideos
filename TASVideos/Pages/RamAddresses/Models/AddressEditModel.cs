using System.ComponentModel.DataAnnotations;
using System.Globalization;
using TASVideos.Data.Entity.Game;
using TASVideos.Models.ValidationAttributes;

namespace TASVideos.Pages.RamAddresses.Models;

public class AddressEditModel
{
	public int Id { get; set; }

	[Required]
	[Display(Name = "Domain")]
	public int GameRamAddressDomainId { get; set; }

	public long Address
	{
		get
		{
			var result = long.TryParse(FormattedAddress, NumberStyles.HexNumber, null, out long longVal);
			if (result)
			{
				return longVal;
			}

			return 0;
		}

		set => FormattedAddress = FormatAddress(value);
	}

	[Required]
	[HexNumber]
	public string? FormattedAddress { get; set; }

	[Required]
	public RamAddressType? Type { get; set; }

	[Required]
	public RamAddressSigned? Signed { get; set; }

	[Required]
	public RamAddressEndian? Endian { get; set; }

	[Required]
	[StringLength(255)]
	public string Description { get; set; } = "";

	public int GameId { get; set; }
	public string GameName { get; set; } = "";
	public string SystemCode { get; set; } = "";
	public int SystemId { get; set; }

	private static string FormatAddress(long address)
	{
		if (address < 0x10000)
		{
			return address.ToString("X4");
		}

		if (address < 0x1000000)
		{
			return address.ToString("X6");
		}

		if (address < 0x100000000)
		{
			return address.ToString("X8");
		}

		return address.ToString("X16");
	}
}
