using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace TASVideos.ModelBinding;

// TODO: cleanup and break up
// https://github.com/aspnet/Mvc/issues/6215
public class DelimitedQueryStringValueProvider(
	BindingSource bindingSource,
	IQueryCollection values,
	CultureInfo culture,
	char[] delimiters)
	: QueryStringValueProvider(bindingSource, values, culture)
{
	private readonly CultureInfo _culture = culture;
	private readonly IQueryCollection _queryCollection = values;

	public char[] Delimiters { get; } = delimiters;

	public override ValueProviderResult GetValue(string key)
	{
		var values = _queryCollection[key];
		if (values.Count == 0)
		{
			return ValueProviderResult.None;
		}

		if (values.Any(x => Delimiters.Any((x ?? "").Contains)))
		{
			var stringValues = new StringValues(values
				.SelectMany(x => (x ?? "").Split(Delimiters, StringSplitOptions.RemoveEmptyEntries))
				.ToArray());
			return new ValueProviderResult(stringValues, _culture);
		}

		return new ValueProviderResult(values, _culture);
	}
}

/// <summary>
/// A <see cref="IValueProviderFactory"/> that creates <see cref="IValueProvider"/> instances that
/// read optionally delimited values from the request query-string.
/// </summary>
public class DelimitedQueryStringValueProviderFactory(params char[] delimiters) : IValueProviderFactory
{
	private static readonly char[] DefaultDelimiters = [','];
	private readonly char[] _delimiters = delimiters.Length == 0
		? DefaultDelimiters
		: delimiters;

	public DelimitedQueryStringValueProviderFactory()
		: this(DefaultDelimiters)
	{
	}

	/// <inheritdoc />
	public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var valueProvider = new DelimitedQueryStringValueProvider(
			BindingSource.Query,
			context.ActionContext.HttpContext.Request.Query,
			CultureInfo.InvariantCulture,
			_delimiters);

		context.ValueProviders.Add(valueProvider);

		return Task.CompletedTask;
	}
}

public static class ValueProviderFactoriesExtensions
{
	public static void AddDelimitedValueProviderFactory(
		this IList<IValueProviderFactory> valueProviderFactories,
		params char[] delimiters)
	{
		var queryStringValueProviderFactory = valueProviderFactories
			.OfType<QueryStringValueProviderFactory>()
			.FirstOrDefault();
		if (queryStringValueProviderFactory is null)
		{
			valueProviderFactories.Insert(
				0,
				new DelimitedQueryStringValueProviderFactory(delimiters));
		}
		else
		{
			valueProviderFactories.Insert(
				valueProviderFactories.IndexOf(queryStringValueProviderFactory),
				new DelimitedQueryStringValueProviderFactory(delimiters));
			valueProviderFactories.Remove(queryStringValueProviderFactory);
		}
	}
}
