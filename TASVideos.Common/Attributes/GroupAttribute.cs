namespace TASVideos.Attributes;

/// <summary>
/// Represents a generic group that a class or property might belong
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class GroupAttribute : Attribute
{
	public GroupAttribute(string name)
	{
		Name = name;
	}

	public string Name { get; }
}
