using FluentValidation;

namespace TASVideos.Api.Validators;

public class TagAddEditRequestValidator : AbstractValidator<TagAddEditRequest>
{
	public TagAddEditRequestValidator()
	{
		RuleFor(t => t.Code).MinimumLength(1).MaximumLength(25);
		RuleFor(t => t.DisplayName).MinimumLength(1).MaximumLength(50);
	}
}
