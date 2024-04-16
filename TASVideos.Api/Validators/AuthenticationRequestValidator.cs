using FluentValidation;

namespace TASVideos.Api.Validators;
public class AuthenticationRequestValidator : AbstractValidator<AuthenticationRequest>
{
	public AuthenticationRequestValidator()
	{
		RuleFor(g => g.Username).MinimumLength(1);
		RuleFor(g => g.Password).MinimumLength(1);
	}
}
