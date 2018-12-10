namespace TASVideos.MovieParsers.Result
{
	/// <summary>
	/// Indicates the region type used in by the parsed movie
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
	/// Indicates a problem with the parsed movie but was not considered
	/// an error.
	/// </summary>
	/// <seealso cref="IParseResult"/>
	public enum ParseWarnings
	{
		/// <summary>
		/// Indicates that the rerecord count was missing in the movie file. 
		/// </summary>
		MissingRerecordCount
	}
}
