﻿using System.Collections.Generic;

namespace TASVideos.MovieParsers.Result;

/// <summary>
/// Represents the result of parsing a movie file.
/// </summary>
/// <seealso cref="IMovieParser"/>
public interface IParseResult
{
	/// <summary>
	/// Gets a value indicating whether or not the movie was successfully parsed
	/// If not successful the <see cref="Errors"/> property will contain at least one error.
	/// </summary>
	bool Success { get; }

	/// <summary>
	/// Gets a list of errors that occurred during the parsing attempt. If there are any errors
	/// the result is considered not successful and values are incorrect or not available.
	/// </summary>
	/// <seealso cref="Success"/>
	IEnumerable<string> Errors { get; } // If success is false, errors should exist

	/// <summary>
	/// Gets a list of warnings that occurred during the parsing attempt. Warnings are issues
	/// that are not serious enough to consider the result a failure but are otherwise not
	/// ideal; for example, a missing rerecord count.
	/// </summary>
	IEnumerable<ParseWarnings> Warnings { get; }

	/// <summary>
	/// Gets the file extension of the movie file, ex: .bk2, .fm2, etc.
	/// </summary>
	string FileExtension { get; }

	/// <summary>
	/// Gets the region used in the movie.
	/// </summary>
	RegionType Region { get; }

	/// <summary>
	/// Gets frame length of the movie.
	/// </summary>
	int Frames { get; }

	/// <summary>
	/// Gets the system code for the movie, ex: NES, SNES, Genesis, etc.
	/// </summary>
	string SystemCode { get; }

	/// <summary>
	/// Gets the rerecord count of the movie.
	/// </summary>
	int RerecordCount { get; }

	/// <summary>
	/// Gets the start type of the movie,
	/// ex: power-on, sram, savestate.
	/// </summary>
	MovieStartType StartType { get; }

	/// <summary>
	/// Gets the framerate reported by the movie format. Formats for systems that have a high number
	/// of framerates will report this value
	/// If null, the calling system should infer the frame rate based on system and region.
	/// </summary>
	double? FrameRateOverride { get; }

	/// <summary>
	/// Gets the cycle count provided in the movie file, if available. Only certain formats store this information.
	/// </summary>
	long? CycleCount { get; }
}
