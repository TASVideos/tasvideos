using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class PostHelperTagHelper : TagHelper
{
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		ViewContext.ViewData.UsePostHelper();
		output.TagName = "div";
		output.AddCssClass("row");
		output.Content.AppendHtml(
			$"""
			<div class="col-12">
				<div class="btn-group mt-1 mb-1">
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[b],[/b]" data-key="b"><strong>b</strong></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[i],[/i]" data-key="i"><i>i</i></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[u],[/u]" data-key="u"><u>u</u></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[s],[/s]" data-key="k"><s>s</s></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[size=],[/size]">size</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[sub],[/sub]" data-key="="><sub>sub</sub></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[sup],[/sup]" data-skey="+"><sup>sup</sup></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[color=&quot;&quot;],[/color]">color</button>
				</div>

				<div class="btn-group mt-1 mb-1 flex-wrap">
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[quote],[/quote]" data-akey="q">quote</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[url],[/url]" data-akey="w">url</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[email],[/email]" data-akey="e">email</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[img],[/img]">img</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[code],[/code]" data-akey="c">code</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[noparse],[/noparse]">noparse</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[spoiler],[/spoiler]">spoiler</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[warning],[/warning]">warning</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[note],[/note]">note</button>
					<button type="button" class="btn btn-info btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[highlight],[/highlight]">highlight</button>
				</div>

				<div class="btn-group mt-1 mb-1">
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[tt],[/tt]">tt</button>
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[list][*],[/list]">&#8226;</button>
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[list=1][*],[/list]">1.</button>
				</div>

				<div class="btn-group mt-1 mb-1">
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[left],[/left]">left</button>
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[right],[/right]">right</button>
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[center],[/center]">center</button>
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[table][tr][td],[/td][/tr][/table]">table</button>
					<button type="button" class="btn btn-secondary btn-sm border-dark" tabindex="-1" data-fmt="[hr],">hr</button>
				</div>

				<div class="btn-group mt-1 mb-1 flex-wrap">
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[frames],[/frames]">frames</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[video],[/video]" data-akey="v">video</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[google],[/google]">google</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[thread],[/thread]">thread</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[post],[/post]">post</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[game],[/game]">game</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[gamegroup],[/gamegroup]">gamegroup</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[movie],[/movie]">movie</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[submission],[/submission]">submission</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[wiki],[/wiki]">wiki</button>
					<button type="button" class="btn btn-success btn-sm flex-grow-0 border-dark" tabindex="-1" data-fmt="[userfile],[/userfile]">userfile</button>
				</div>
				<div class="mt-1 mb-2">
					<small><a href="/ForumMarkup" target="_blank">Forum Markup Usage</a></small>
				</div>
			</div>
			""");
	}
}
