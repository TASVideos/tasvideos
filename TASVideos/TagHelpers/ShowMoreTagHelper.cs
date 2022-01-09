using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	public class ShowMoreTagHelper : TagHelper
	{
		public string MaxHeight { get; set; } = "none";
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var content = (await output.GetChildContentAsync()).GetContent();
			output.TagName = "div";
			output.Content.SetHtmlContent($@"<div style=""overflow-y: scroll; max-height: {MaxHeight};"" id=""{context.UniqueId}"">{content}</div>");
			output.Content.AppendHtml($@"<div class=""p-2 text-center d-none border border-primary rounded"" id=""show-{context.UniqueId}""><a href=""#"" onclick=""return false;""><h4 class=""m-0""><i class=""fa fa-chevron-down""></i> Show more</h4></a></div>");
			output.Content.AppendHtml($@"
<script>
	{{
		let content = document.getElementById(""{context.UniqueId}"");
		if (content.scrollHeight > content.clientHeight)
			{{
				let show = document.getElementById('show-{context.UniqueId}');
				content.style.overflowY = 'hidden'
				show.classList.remove('d-none');
				show.onclick = function()
				{{
					content.style.overflowY = null;
					content.style.maxHeight = null;
					show.classList.add('d-none');
				}}
			}}
	}}
</script>");
		}
	}
}
