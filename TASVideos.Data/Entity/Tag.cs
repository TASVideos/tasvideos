﻿namespace TASVideos.Data.Entity;

public class Tag
{
	public int Id { get; set; }

	public string Code { get; set; } = "";

	public string DisplayName { get; set; } = "";

	public ICollection<PublicationTag> PublicationTags { get; init; } = [];
}
