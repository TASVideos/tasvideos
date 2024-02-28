﻿namespace TASVideos.Data.Entity;

public class Tag
{
	public int Id { get; set; }

	[StringLength(25)]
	public string Code { get; set; } = "";

	[StringLength(50)]
	public string DisplayName { get; set; } = "";

	public virtual ICollection<PublicationTag> PublicationTags { get; set; } = new HashSet<PublicationTag>();
}
