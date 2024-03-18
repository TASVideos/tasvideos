namespace TASVideos.Core.Services.Email;

/// <summary>
/// Represents an email that can be sent to an <see cref="IEmailSender"/>
/// </summary>
public interface IEmail
{
	/// <summary>
	/// Gets the recipients of the email
	/// </summary>
	IEnumerable<string> Recipients { get; }

	/// <summary>
	/// Gets the subject of the email
	/// </summary>
	string Subject { get; }

	/// <summary>
	/// Gets the contents of the email
	/// </summary>
	string Message { get; }

	/// <summary>
	/// Gets a value indicating whether the <see cref="Message" /> contains HTML content
	/// </summary>
	bool ContainsHtml { get; }
}

public class SingleEmail : IEmail
{
	public string Recipient { get; init; } = "";
	public IEnumerable<string> Recipients => [Recipient];
	public string Subject { get; init; } = "";
	public string Message { get; init; } = "";
	public bool ContainsHtml { get; init; }
}

public class StandardEmail : IEmail
{
	public IEnumerable<string> Recipients { get; init; } = [];
	public string Subject { get; init; } = "";
	public string Message { get; init; } = "";
	public bool ContainsHtml { get; init; }
}
