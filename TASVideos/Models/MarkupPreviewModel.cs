namespace TASVideos.Models
{
	public record MarkupPreviewModel
	{
		/// <summary>
		/// Html ID of the textbox that contains the markup in it
		/// </summary>
		/// <value></value>
		public string MarkupEditorId { get; }
		/// <summary>
		/// Relative path to POST the markup to to get a server rendering of it
		/// </summary>
		/// <value></value>
		public string AjaxPath { get; }

		public MarkupPreviewModel(string markupEditorId, string ajaxPath)
		{
			MarkupEditorId = markupEditorId;
			AjaxPath = ajaxPath;
		}
	}
}
