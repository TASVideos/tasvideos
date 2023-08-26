using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class SelectImproverTagHelper : TagHelper
{
	public string SelectId { get; set; } = "";
	public string ListHeight { get; set; } = "250px";
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "div";
		output.Content.SetHtmlContent(@$"
<div id=""{SelectId}_div"" class=""d-none border bg-body rounded-2 onclick-focusinput"" style=""cursor: text;"">
	<div id=""{SelectId}_buttons"" class=""onclick-focusinput px-2 py-2"">
		<span id=""{SelectId}_noselection"" class=""text-body-tertiary onclick-focusinput px-1"">No selection</span>
	</div>
	<input id=""{SelectId}_input"" class=""d-none form-control"" placeholder=""Search"" />
	<div id=""{SelectId}_list"" class=""list-group mt-1 overflow-auto"" style=""max-height: {ListHeight};""></div>
</div>
<script>
	function toggle(multiSelect, buttons, list, value) {{
		let element = [...list.querySelectorAll('a')].find(el => el.dataset.value === value);
		let option = [...multiSelect.options].find(option => option.value === value);
		let button = [...buttons.querySelectorAll('button')].find(button => button.dataset.value === value);
		let isSelected = option.selected;
		if (isSelected) {{
			option.selected = false;
			element.classList.remove('active');
			button.classList.add('d-none');
			if (![...multiSelect.options].some(option => option.selected)) {{
				buttons.querySelector('span').classList.remove('d-none');
			}}
		}}
		else {{
			option.selected = true;
			element.classList.add('active');
			button.classList.remove('d-none');
			buttons.querySelector('span').classList.add('d-none');
		}}
	}}
	function initialize(multiSelectId) {{
		let multiSelect = document.getElementById(multiSelectId);
		multiSelect.classList.add('d-none');
		let list = document.getElementById(multiSelectId + '_list');
		let buttons = document.getElementById(multiSelectId + '_buttons');
		let div = document.getElementById(multiSelectId + '_div');
		div.classList.remove('d-none');
		let input = document.getElementById(multiSelectId + '_input');
		for (var option of multiSelect.options) {{
			let entry = document.createElement('a');
			entry.classList.add('list-group-item', 'list-group-item-action', 'py-1');
			if (option.selected) {{
				entry.classList.add('active');
			}}
			entry.style.cursor = 'pointer';
			entry.innerText = option.text;
			entry.dataset.value = option.value;
			entry.addEventListener('click', (e) => {{ toggle(multiSelect, buttons, list, e.currentTarget.dataset.value); }});
			list.appendChild(entry);

			let button = document.createElement('button');
			button.type = 'button';
			button.classList.add('btn', 'btn-primary', 'btn-sm', 'm-1');
			button.dataset.value = option.value;
			if (option.selected) {{
				buttons.querySelector('span').classList.add('d-none');
			}} else {{
				button.classList.add('d-none');
			}}
			button.addEventListener('click', (e) => {{ toggle(multiSelect, buttons, list, e.currentTarget.dataset.value); }});
			let buttonSpanText = document.createElement('span');
			buttonSpanText.innerText = option.text;
			button.appendChild(buttonSpanText);
			let buttonSpanX = document.createElement('span');
			buttonSpanX.innerText = '✕'
			buttonSpanX.classList.add('ps-1');
			button.appendChild(buttonSpanX);

			buttons.appendChild(button);
		}}
		div.addEventListener('click', (e) => {{
			if (e.target.classList.contains('onclick-focusinput')) {{
				input.classList.remove('d-none');
				input.focus();
			}}
		}});
		input.addEventListener('input', (e) => {{
			let searchValue = input.value;
			for (let entry of list.querySelectorAll('a')) {{
				if (entry.innerText.includes(searchValue)) {{
					entry.classList.remove('d-none');
				}}
				else {{
					entry.classList.add('d-none');
				}}
			}}
		}});
		input.addEventListener('focusout', (e) => {{
			if (!input.value) {{
				input.classList.add('d-none');
			}}
		}});
	}}
	initialize('{SelectId}');
</script>
");
	}
}
