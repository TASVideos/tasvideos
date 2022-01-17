using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	public class ShowMoreTagHelper : TagHelper
	{
		public string MaxHeight { get; set; } = "none";
		public string ShowText { get; set; } = "Show more";
		public string HideText { get; set; } = "Hide";
		public bool Reverse { get; set; } = false;
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var content = (await output.GetChildContentAsync()).GetContent();
			output.TagName = "div";
			output.Content.SetHtmlContent($@"<div style=""overflow-y: scroll; max-height: {MaxHeight};"" id=""{context.UniqueId}"">{content}</div>");
			output.Content.AppendHtml($@"<div class=""p-2 text-center d-none border border-primary rounded"" id=""show-{context.UniqueId}""><a href=""#"" onclick=""return false;""><h4 class=""m-0""><i class=""fa fa-chevron-{(Reverse ? "up" : "down")}""></i> {ShowText}</h4></a></div>");
			output.Content.AppendHtml($@"<div class=""p-2 text-center d-none border border-primary rounded"" id=""hide-{context.UniqueId}""><a href=""#"" onclick=""return false;""><h4 class=""m-0""><i class=""fa fa-chevron-{(Reverse ? "down" : "up")}""></i> {HideText}</h4></a></div>");
			output.Content.AppendHtml($@"
<script>
	{{
		let content = document.getElementById(""{context.UniqueId}"");
		{(Reverse ? "content.scrollTop = 1e10;" : "")}
		if (content.scrollHeight > content.clientHeight)
			{{
				let show = document.getElementById('show-{context.UniqueId}');
				let hide = document.getElementById('hide-{context.UniqueId}');
				let height = content.style.maxHeight;
				let clHeight = content.clientHeight;
				content.style.overflowY = 'hidden'
				show.classList.remove('d-none');
				show.onclick = function()
				{{
					let scroll = content.scrollTop;
					content.style.overflowY = null;
					content.style.maxHeight = null;
					show.classList.add('d-none');
					hide.classList.remove('d-none');
					window.scrollBy({{top: scroll, left: 0, behavior: 'instant'}});
				}}
				hide.onclick = function()
				{{
					window.scrollBy({{top: clHeight - content.scrollHeight, left: 0, behavior: 'instant'}});
					content.style.overflowY = 'hidden';
					content.style.maxHeight = height;
					hide.classList.add('d-none');
					show.classList.remove('d-none');
					content.scrollTop = {(Reverse ? "1e10" : "0")};
				}}
			}}
	}}
</script>");
		}
	}
}
