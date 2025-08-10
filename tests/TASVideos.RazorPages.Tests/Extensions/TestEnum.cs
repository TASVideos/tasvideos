using System.ComponentModel.DataAnnotations;

namespace TASVideos.RazorPages.Tests.Extensions;

internal enum TestEnum
{
	[System.ComponentModel.Description("00")]
	Zero,

	[Display(Name = "The One", Description = "01")]
	One,

	Two
}
