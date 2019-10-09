using Microsoft.AspNetCore.Mvc.ModelBinding;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Extensions
{
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
	}
}
