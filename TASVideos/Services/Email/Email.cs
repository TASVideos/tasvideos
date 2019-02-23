using System.Collections.Generic;

namespace TASVideos.Services.Email
{
	/// <summary>
	/// Represents an email that can be send to an <see cref="IEmailSender"/>
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
	}

	public class SingleEmail : IEmail
	{
		public string Recipient { get; set; }
		public IEnumerable<string> Recipients => new[] { Recipient };
		public string Subject { get; set; }
		public string Message { get; set; }
	}

	public class StandardEmail : IEmail
	{
		public IEnumerable<string> Recipients { get; set; } = new string[0];
		public string Subject { get; set; }
		public string Message { get; set; }
	}
}
