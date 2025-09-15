using Microsoft.Extensions.Hosting;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class EmailServiceTests : TestDbBase
{
	private readonly IEmailSender _emailSender;
	private readonly IHostEnvironment _hostEnvironment;
	private readonly AppSettings _appSettings;
	private readonly EmailService _emailService;

	public EmailServiceTests()
	{
		_emailSender = Substitute.For<IEmailSender>();
		_hostEnvironment = Substitute.For<IHostEnvironment>();
		_appSettings = new AppSettings
		{
			BaseUrl = "https://tasvideos.org"
		};

		_emailService = new EmailService(_hostEnvironment, _emailSender, _appSettings);
	}

	[TestMethod]
	public async Task SendEmail_ValidInputs_SendsEmailWithCorrectRecipient()
	{
		const string recipient = "test@example.com";
		const string subject = "Test Subject";
		const string message = "Test Message";

		await _emailService.SendEmail(recipient, subject, message);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Recipients.Single() == recipient
			&& !string.IsNullOrEmpty(email.Subject)
			&& !string.IsNullOrEmpty(email.Message)
			&& email.ContainsHtml == false));
	}

	[TestMethod]
	public async Task ResetPassword_ValidInputs_SendsEmailWithCorrectRecipientAndContent()
	{
		const string recipient = "user@example.com";
		const string link = "https://tasvideos.org/reset?token=abc123";
		const string userName = "TestUser";

		await _emailService.ResetPassword(recipient, link, userName);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Recipients.Single() == recipient
				&& !string.IsNullOrEmpty(email.Subject)
				&& !string.IsNullOrEmpty(email.Message)
				&& email.Message.Contains(userName)
				&& email.Message.Contains(link)
				&& email.ContainsHtml == true));
	}

	[TestMethod]
	public async Task ResetPassword_WithSpecialCharacters_EncodesContentProperly()
	{
		const string recipient = "user@example.com";
		const string link = "https://tasvideos.org/reset?token=<script>alert('xss')</script>";
		const string userName = "<script>alert('xss')</script>";

		await _emailService.ResetPassword(recipient, link, userName);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Recipients.Single() == recipient
				&& !email.Message.Contains("<script>") // Should be HTML encoded
				&& email.Message.Contains("&lt;script&gt;"))); // Should contain the encoded version
	}

	[TestMethod]
	public async Task EmailConfirmation_ValidInputs_SendsEmailWithCorrectRecipientAndContent()
	{
		const string recipient = "user@example.com";
		const string link = "https://tasvideos.org/confirm?token=xyz789";

		await _emailService.EmailConfirmation(recipient, link);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Recipients.Single() == recipient
				&& !string.IsNullOrEmpty(email.Subject)
				&& !string.IsNullOrEmpty(email.Message)
				&& email.Message.Contains(link)
				&& email.ContainsHtml == true));
	}

	[TestMethod]
	public async Task PasswordResetConfirmation_ValidInputs_SendsEmailWithCorrectRecipientAndContent()
	{
		const string recipient = "user@example.com";
		const string resetLink = "https://tasvideos.org/reset?token=def456";

		await _emailService.PasswordResetConfirmation(recipient, resetLink);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Recipients.Single() == recipient
			&& !string.IsNullOrEmpty(email.Subject)
			&& !string.IsNullOrEmpty(email.Message)
			&& email.Message.Contains(resetLink)
			&& email.ContainsHtml == true));
	}

	[TestMethod]
	public async Task TopicReplyNotification_SingleRecipient_SendsEmailWithCorrectRecipient()
	{
		var recipients = new[] { "user1@example.com" };
		var template = new TopicReplyNotificationTemplate(
			PostId: 123,
			TopicId: 456,
			TopicTitle: "Test Topic",
			BaseUrl: "https://tasvideos.org");

		_hostEnvironment.EnvironmentName.Returns("Production");

		await _emailService.TopicReplyNotification(recipients, template);

		await _emailSender.Received(1).SendEmail(Arg.Is<StandardEmail>(email =>
			email.Recipients.Single() == recipients[0]
				&& !string.IsNullOrEmpty(email.Subject)
				&& !string.IsNullOrEmpty(email.Message)
				&& email.Message.Contains(template.TopicTitle)
				&& email.ContainsHtml == true));
	}

	[TestMethod]
	public async Task TopicReplyNotification_MultipleRecipients_SendsEmailToAllRecipients()
	{
		var recipients = new[] { "user1@example.com", "user2@example.com", "user3@example.com" };
		var template = new TopicReplyNotificationTemplate(
			PostId: 123,
			TopicId: 456,
			TopicTitle: "Test Topic",
			BaseUrl: "https://tasvideos.org");

		_hostEnvironment.EnvironmentName.Returns("Production");

		await _emailService.TopicReplyNotification(recipients, template);

		await _emailSender.Received(1).SendEmail(Arg.Is<StandardEmail>(email =>
			email.Recipients.Count() == 3
				&& email.Recipients.Contains(recipients[0])
				&& email.Recipients.Contains(recipients[1])
				&& email.Recipients.Contains(recipients[2])
				&& !string.IsNullOrEmpty(email.Subject)
				&& !string.IsNullOrEmpty(email.Message)));
	}

	[TestMethod]
	public async Task TopicReplyNotification_EmptyRecipients_DoesNotSendEmail()
	{
		var recipients = Array.Empty<string>();
		var template = new TopicReplyNotificationTemplate(
			PostId: 123,
			TopicId: 456,
			TopicTitle: "Test Topic",
			BaseUrl: "https://tasvideos.org");

		await _emailService.TopicReplyNotification(recipients, template);

		await _emailSender.DidNotReceive().SendEmail(Arg.Any<IEmail>());
	}

	[TestMethod]
	public async Task TopicReplyNotification_NonProductionEnvironment_IncludesEnvironmentInSubject()
	{
		var recipients = new[] { "user@example.com" };
		var template = new TopicReplyNotificationTemplate(
			PostId: 123,
			TopicId: 456,
			TopicTitle: "Test Topic",
			BaseUrl: "https://tasvideos.org");

		_hostEnvironment.EnvironmentName.Returns("Development");

		await _emailService.TopicReplyNotification(recipients, template);

		await _emailSender.Received(1).SendEmail(Arg.Is<StandardEmail>(email =>
			email.Message.Contains("TASVideos - Development environment")));
	}

	[TestMethod]
	public async Task TopicReplyNotification_WithSpecialCharactersInTitle_EncodesTitle()
	{
		var recipients = new[] { "user@example.com" };
		var template = new TopicReplyNotificationTemplate(
			PostId: 123,
			TopicId: 456,
			TopicTitle: "<script>alert('xss')</script>",
			BaseUrl: "https://tasvideos.org");

		_hostEnvironment.EnvironmentName.Returns("Production");

		await _emailService.TopicReplyNotification(recipients, template);

		await _emailSender.Received(1).SendEmail(Arg.Is<StandardEmail>(email =>
			!email.Message.Contains("<script>") // Should be HTML encoded
				&& email.Message.Contains("&lt;script&gt;"))); // Should contain the encoded version
	}

	[TestMethod]
	public async Task NewPrivateMessage_ValidInputs_SendsEmailWithCorrectRecipientAndContent()
	{
		const string recipient = "user@example.com";
		const string userName = "TestUser";

		await _emailService.NewPrivateMessage(recipient, userName);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Recipients.Single() == recipient
				&& !string.IsNullOrEmpty(email.Subject)
				&& !string.IsNullOrEmpty(email.Message)
				&& email.Message.Contains(userName)
				&& email.Message.Contains(_appSettings.BaseUrl)
				&& email.ContainsHtml == false));
	}

	[TestMethod]
	public async Task NewPrivateMessage_UsesBaseUrlFromSettings_IncludesCorrectLink()
	{
		const string recipient = "user@example.com";
		const string userName = "TestUser";
		const string expectedLink = "https://tasvideos.org/Messages/Inbox";

		await _emailService.NewPrivateMessage(recipient, userName);

		await _emailSender.Received(1).SendEmail(Arg.Is<SingleEmail>(email =>
			email.Message.Contains(expectedLink)));
	}
}
