using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace TASVideos.Pages;

[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
	public string RecoveredFormData { get; set; } = "";
	public void OnGet()
	{
		RecoveredFormData = $"Status Code {Response.StatusCode}";
	}

	public void OnPost()
	{
		var stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Status Code {Response.StatusCode}");
		stringBuilder.AppendLine("========");
		foreach (var kvp in Request.Form)
		{
			stringBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
			stringBuilder.AppendLine("========");
		}

		RecoveredFormData = stringBuilder.ToString();
	}
}
