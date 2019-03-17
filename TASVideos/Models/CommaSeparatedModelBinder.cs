using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace TASVideos.Models
{
	// TODO: cleanup and break up
	// https://github.com/aspnet/Mvc/issues/6215
	public class DelimitedQueryStringValueProvider : QueryStringValueProvider
	{
		private readonly CultureInfo _culture;
		private readonly IQueryCollection _queryCollection;

		public DelimitedQueryStringValueProvider(
			BindingSource bindingSource,
			IQueryCollection values,
			CultureInfo culture,
			char[] delimiters)
			: base(bindingSource, values, culture)
		{
			_queryCollection = values;
			_culture = culture;
			Delimiters = delimiters;
		}

		public char[] Delimiters { get; }

		public override ValueProviderResult GetValue(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			var values = _queryCollection[key];
			if (values.Count == 0)
			{
				return ValueProviderResult.None;
			}

			if (values.Any(x => Delimiters.Any(x.Contains)))
			{
				var stringValues = new StringValues(values
					.SelectMany(x => x.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries))
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
	public class DelimitedQueryStringValueProviderFactory : IValueProviderFactory
	{
		private static readonly char[] DefaultDelimiters = { ',' };
		private readonly char[] _delimiters;

		public DelimitedQueryStringValueProviderFactory()
			: this(DefaultDelimiters)
		{
		}

		public DelimitedQueryStringValueProviderFactory(params char[] delimiters)
		{
			if (delimiters == null || delimiters.Length == 0)
			{
				_delimiters = DefaultDelimiters;
			}
			else
			{
				_delimiters = delimiters;
			}
		}

		/// <inheritdoc />
		public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

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
			if (queryStringValueProviderFactory == null)
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
}
