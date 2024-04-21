namespace TASVideos.Api.Responses;

// Only used for documentation because swagger cannot handle the fact that the entity object has a circular reference
internal class TagsResponse
{
	public int Id { get; set; }
	public string Code { get; set; } = "";
	public string DisplayName { get; set; } = "";
}
