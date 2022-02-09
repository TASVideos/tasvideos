// ReSharper disable InconsistentNaming
namespace TASVideos.Data.Entity.Game;

public enum RamAddressType
{
	Byte,
	Word,

	[Display(Name = "Double Word")]
	DWord,
	Float,

	[Display(Name = "Fixed Point 12.4")]
	Q12_4,

	[Display(Name = "Fixed Point 20.12")]
	Q20_12,

	[Display(Name = "Fixed Point 20.4")]
	Q20_4,

	[Display(Name = "Fixed Point 28.4")]
	Q28_4,

	[Display(Name = "Fixed Point 8.8")]
	Q8_8,

	[Display(Name = "Fixed Point 16.8")]
	Q16_8,

	[Display(Name = "Fixed Point 24.8")]
	Q24_8,

	[Display(Name = "Fixed Point 16.16")]
	Q16_16,

	[Display(Name = "3 Byte")]
	ThreeByte
}

public enum RamAddressSigned
{
	Signed,
	Unsigned,
	Hex
}

public enum RamAddressEndian
{
	Big,
	Little,
	Host
}

public class GameRamAddress
{
	public int Id { get; set; }

	public int LegacySetId { get; set; }
	public long Address { get; set; }
	public RamAddressType Type { get; set; }
	public RamAddressSigned Signed { get; set; }
	public RamAddressEndian Endian { get; set; }

	[StringLength(255)]
	public string Description { get; set; } = "";

	public int GameRamAddressDomainId { get; set; }
	public virtual GameRamAddressDomain? GameRamAddressDomain { get; set; }

	public int? GameId { get; set; }
	public virtual Game? Game { get; set; }

	[StringLength(255)]
	public string? LegacyGameName { get; set; }

	public int SystemId { get; set; }
	public virtual GameSystem? System { get; set; }
}
