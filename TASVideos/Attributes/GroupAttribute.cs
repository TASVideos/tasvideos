using System;

namespace TASVideos.Attributes
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
