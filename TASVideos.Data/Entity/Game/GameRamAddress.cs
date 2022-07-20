// ReSharper disable InconsistentNaming
namespace TASVideos.Data.Entity.Game;

public enum RamAddressType
{
	Byte,
	Word,
	DWord,
	Float,
	Q12_4,
	Q20_12,
	Q20_4,
	Q28_4,
	Q8_8,
	Q16_8,
	Q24_8,
	Q16_16,
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
