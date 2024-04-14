using FluentValidation;

namespace TASVideos.Api.Validators;

public class ApiRequestValidator : AbstractValidator<ApiRequest>
{
	public ApiRequestValidator()
	{
		RuleFor(a => a.PageSize).Must(ps => ps is null or >= 1 and <= ApiConstants.MaxPageSize).WithMessage("Invalid page size");
		RuleFor(a => a.CurrentPage).Must(c => c is null or >= 1).WithMessage("Invalid current page");
		RuleFor(a => a.Sort).MaximumLength(200);
		RuleFor(a => a.Fields).MaximumLength(200);
	}
}