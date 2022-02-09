namespace TASVideos.MovieParsers.Result;

/// <summary>
/// Indicates the region type used in by the parsed movie.
/// </summary>
/// /// <seealso cref="IParseResult"/>
public enum RegionType
{
	/// <summary>
	/// The region could not be determined
	/// </summary>
	Unknown,

	/// <summary>
	/// Indicates that the region is the NTSC standard
	/// </summary>
	Ntsc,

	/// <summary>
	/// Indicates that the region is the PAL standard
	/// </summary>
	Pal
}

/// <summary>
/// Indicates the starting state for the movie,
/// power-on, dirty SRAM, savestate, etc.
/// </summary>
public enum MovieStartType
{
	/// <summary>
	/// The movie starts from a full power-cycle with clean SRAM
	/// </summary>
	PowerOn,

	/// <summary>
	/// The movie starts from a power-cycle with dirty SRAM
	/// </summary>
	Sram,

	/// <summary>
	/// The movie starts from a savestate
	/// </summary>
	Savestate
}

/// <summary>
/// Indicates a problem with the parsed movie but was not considered
/// an error.
/// </summary>
/// <seealso cref="IParseResult"/>
public enum ParseWarnings
{
	/// <summary>
	/// Indicates that the rerecord count was missing in the movie file.
	/// </summary>
	MissingRerecordCount,

	/// <summary>
	/// Indicates that the system id could not be reliably determined, and a default was used
	/// </summary>
	SystemIdInferred,

	/// <summary>
	/// Indicates that the region could not be reliably determined, and a default was used.
	/// </summary>
	RegionInferred,

	/// <summary>
	/// Indicates that a framerate override was needed but no value could be found, and a default was used
	/// </summary>
	FrameRateInferred,

	/// <summary>
	/// Indicates that the movie length could not be accurately determined, due to missing data or legacy format
	/// </summary>
	LengthInferred
}
