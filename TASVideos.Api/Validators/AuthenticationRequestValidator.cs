namespace TASVideos.Api.Validators;

internal class AuthenticationRequestValidator : AbstractValidator<AuthenticationRequest>
{
	public AuthenticationRequestValidator()
	{
		RuleFor(a => a.Username).MinimumLength(1);
		RuleFor(a => a.Password).MinimumLength(1);
	}
}
