using System.Xml.Serialization;

// https://xmltocsharp.azurewebsites.net/
namespace TASVideos.Core.Services.RssFeedParsers.Github;

[XmlRoot(ElementName = "feed", Namespace = "http://www.w3.org/2005/Atom")]
public class GithubFeedResult
{
	[XmlElement(ElementName = "id", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Id { get; set; }

	[XmlElement(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
	public List<Link> Link { get; set; } = new();

	[XmlElement(ElementName = "title", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Title { get; set; }

	[XmlElement(ElementName = "updated", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Updated { get; set; }

	[XmlElement(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
	public List<Entry> Entry { get; set; } = new();

	[XmlAttribute(AttributeName = "xmlns")]
	public string? Xmlns { get; set; }

	[XmlAttribute(AttributeName = "media", Namespace = "http://www.w3.org/2000/xmlns/")]
	public string? Media { get; set; }

	[XmlAttribute(AttributeName = "lang", Namespace = "http://www.w3.org/XML/1998/namespace")]
	public string? Lang { get; set; }
}

[XmlRoot(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
public class Link
{
	[XmlAttribute(AttributeName = "type")]
	public string? Type { get; set; }

	[XmlAttribute(AttributeName = "rel")]
	public string? Rel { get; set; }

	[XmlAttribute(AttributeName = "href")]
	public string? Href { get; set; }
}

[XmlRoot(ElementName = "thumbnail", Namespace = "http://search.yahoo.com/mrss/")]
public class Thumbnail
{
	[XmlAttribute(AttributeName = "height")]
	public string? Height { get; set; }

	[XmlAttribute(AttributeName = "width")]
	public string? Width { get; set; }

	[XmlAttribute(AttributeName = "url")]
	public string? Url { get; set; }
}

[XmlRoot(ElementName = "author", Namespace = "http://www.w3.org/2005/Atom")]
public class Author
{
	[XmlElement(ElementName = "name", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Name { get; set; }

	[XmlElement(ElementName = "uri", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Uri { get; set; }
}

[XmlRoot(ElementName = "content", Namespace = "http://www.w3.org/2005/Atom")]
public class Content
{
	[XmlAttribute(AttributeName = "type")]
	public string? Type { get; set; }

	[XmlText]
	public string? Text { get; set; }
}

[XmlRoot(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
public class Entry
{
	[XmlElement(ElementName = "id", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Id { get; set; }

	[XmlElement(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
	public Link? Link { get; set; }

	[XmlElement(ElementName = "title", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Title { get; set; }

	[XmlElement(ElementName = "updated", Namespace = "http://www.w3.org/2005/Atom")]
	public string? Updated { get; set; }

	[XmlElement(ElementName = "thumbnail", Namespace = "http://search.yahoo.com/mrss/")]
	public Thumbnail? Thumbnail { get; set; }

	[XmlElement(ElementName = "author", Namespace = "http://www.w3.org/2005/Atom")]
	public Author? Author { get; set; }

	[XmlElement(ElementName = "content", Namespace = "http://www.w3.org/2005/Atom")]
	public Content? Content { get; set; }
}
