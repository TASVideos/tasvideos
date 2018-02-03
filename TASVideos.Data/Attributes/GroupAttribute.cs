using System;

namespace TASVideos.Data.Attributes
{
	public class GroupAttribute : Attribute
	{
		public GroupAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
