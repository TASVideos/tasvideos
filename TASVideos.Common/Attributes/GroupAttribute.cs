namespace TASVideos.Attributes;

/// <summary>
/// Represents a generic group that a class or property might belong
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class GroupAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
