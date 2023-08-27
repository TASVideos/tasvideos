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
		<span class=""text-body-tertiary onclick-focusinput px-1"">No selection</span>
		<a class=""btn btn-sm btn-outline-silver float-end py-0 px-1""></a>
	</div>
	<input id=""{SelectId}_input"" class=""d-none form-control"" placeholder=""Search"" autocomplete=""off"" />
	<div id=""{SelectId}_list"" class=""list-group mt-1 overflow-auto border"" style=""max-height: {ListHeight};""></div>
</div>
<script>
	function toggle(multiSelect, buttons, list, value) {{
		let element = [...list.querySelectorAll('input')].find(el => el.dataset.value === value);
		let option = [...multiSelect.options].find(option => option.value === value);
		let button = [...buttons.querySelectorAll('button')].find(button => button.dataset.value === value);
		let isSelected = option.selected;
		if (isSelected) {{
			option.selected = false;
			multiSelect.dispatchEvent(new Event('change')); // somewhat hacky way to support external event listeners
			element.checked = false;
			button.classList.add('d-none');
			if (![...multiSelect.options].some(option => option.selected)) {{
				buttons.querySelector('span').classList.remove('d-none');
			}}
		}} else {{
			option.selected = true;
			multiSelect.dispatchEvent(new Event('change')); // somewhat hacky way to support external event listeners
			element.checked = true;
			button.classList.remove('d-none');
			buttons.querySelector('span').classList.add('d-none');
		}}
		if ([...multiSelect.options].some(option => !option.selected)) {{
			buttons.querySelector('a').querySelector('i').classList.replace('fa-minus', 'fa-plus');
			buttons.querySelector('a').title = 'Select All';
		}} else {{
			buttons.querySelector('a').querySelector('i').classList.replace('fa-plus', 'fa-minus');
			buttons.querySelector('a').title = 'Deselect All';
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
		let anyNotSelected = false;
		for (var option of multiSelect.options) {{
			let entry = document.createElement('div');
			entry.classList.add('list-group-item', 'list-group-item-action', 'px-1');
			let label = document.createElement('label');
			label.classList.add('form-check-label', 'stretched-link')
			let checkbox = document.createElement('input');
			checkbox.type = 'checkbox';
			checkbox.classList.add('form-check-input', 'ms-1', 'me-2');
			if (option.selected) {{
				checkbox.checked = true;
			}}
			checkbox.dataset.value = option.value;
			checkbox.addEventListener('change', (e) => {{ toggle(multiSelect, buttons, list, e.currentTarget.dataset.value) }});
			label.appendChild(checkbox)
			label.append(option.text);
			entry.appendChild(label);
			list.appendChild(entry);

			let button = document.createElement('button');
			button.type = 'button';
			button.classList.add('btn', 'btn-primary', 'btn-sm', 'mb-1', 'me-1');
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

			if (!option.selected) {{
				anyNotSelected = true;
			}}
		}}
		let toggleAllIcon = document.createElement('i');
		toggleAllIcon.classList.add('fa', 'fa-sm');
		toggleAllIcon.classList.add(anyNotSelected ? 'fa-plus' : 'fa-minus');
		buttons.querySelector('a').appendChild(toggleAllIcon);
		buttons.querySelector('a').title = anyNotSelected ? 'Select All' : 'Deselect All';
		buttons.querySelector('a').addEventListener('click', (e) => {{
			let notSelected = [...multiSelect.options].filter(option => !option.selected);
			if (notSelected.length === 0) {{
				for (let option of multiSelect.options) {{
					toggle(multiSelect, buttons, list, option.value);
				}}
			}} else {{
				for (let option of notSelected) {{
					toggle(multiSelect, buttons, list, option.value);
				}}
			}}
		}});
		div.addEventListener('click', (e) => {{
			if (e.target.classList.contains('onclick-focusinput')) {{
				input.classList.remove('d-none');
				input.focus();
			}}
		}});
		input.addEventListener('input', (e) => {{
			let searchValue = input.value.toLowerCase();
			for (let entry of list.querySelectorAll('label')) {{
				if (entry.innerText.toLowerCase().includes(searchValue)) {{
					entry.parentNode.classList.remove('d-none');
				}} else {{
					entry.parentNode.classList.add('d-none');
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
