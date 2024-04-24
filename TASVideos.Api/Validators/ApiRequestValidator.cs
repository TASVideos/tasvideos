namespace TASVideos.Api.Validators;

internal class ApiRequestValidator : AbstractValidator<ApiRequest>
{
	public ApiRequestValidator()
	{
		this.ValidateApiRequest();
	}
}

internal static class ApiRequestValidatorExtensions
{
	public static void ValidateApiRequest<T>(this AbstractValidator<T> validator, Type? sortType = null)
		where T : ApiRequest
	{
		validator.RuleFor(a => a.PageSize).Must(ps => ps is null or >= 1 and <= ApiRequest.MaxPageSize).WithMessage("Invalid page size");
		validator.RuleFor(a => a.CurrentPage).Must(c => c is null or >= 1).WithMessage("Invalid current page");
		validator.RuleFor(a => a.Sort).MaximumLength(200).Must(sort => sortType is null || sort.IsValidSort(sortType));
		validator.RuleFor(a => a.Fields).MaximumLength(200);
	}
}
