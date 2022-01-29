namespace TASVideos.ViewComponents;

public record AddressEntry(
	string Domain,
	long Address,
	string DataType,
	string Signed,
	string Endian,
	string Description,
	string? GameName,
	int? GameId,
	string? System);
