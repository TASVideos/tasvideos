﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Extensions;

public static class ModelStateExtensions
{
	public static void AddParseErrors(this ModelStateDictionary modelState, IParseResult parseResult, string? modelPropertyName = null)
	{
		if (!parseResult.Success)
		{
			foreach (var error in parseResult.Errors)
			{
				modelState.AddModelError(modelPropertyName ?? "Parser", error);
			}
		}
	}

	public static async Task<byte[]> ToBytes(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return Array.Empty<byte>();
		}

		await using var memoryStream = new MemoryStream();
		await formFile.CopyToAsync(memoryStream);
		return memoryStream.ToArray();
	}
}
