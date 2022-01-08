using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
	public static class HtmlExtensions
	{
		public static bool WikiCondition(ViewContext viewContext, string condition)
		{
			var viewData = viewContext.ViewData;
			bool result = false;

			if (condition.StartsWith('!'))
			{
				result = true;
				condition = condition.TrimStart('!');
			}

			switch (condition)
			{
				default:
					if (Enum.TryParse(condition, out PermissionTo permission))
					{
						result ^= viewData.UserHas(permission);
					}

					break;

				case "CanSubmitMovies": // Legacy system: same as UserIsLoggedIn
				case "CanRateMovies": // Legacy system: same as UserIsLoggedIn
				case "UserIsLoggedIn":
					result ^= viewContext.HttpContext.User.IsLoggedIn();
					break;
				case "1":
					result ^= true;
					break;
				case "0":
					result ^= false;
					break;

				// Support legacy values, these are deprecated
				case "CanEditPages":
					result ^= viewData.UserHas(PermissionTo.EditWikiPages);
					break;
				case "UserHasHomepage":
					result ^= viewContext.HttpContext.User.IsLoggedIn(); // Let's assume every user can have a homepage automatically
					break;
				case "CanViewSubmissions":
					result ^= true; // Legacy system always returned true
					break;
				case "CanJudgeMovies":
					result ^= viewData.UserHas(PermissionTo.JudgeSubmissions);
					break;
				case "CanPublishMovies":
					result ^= viewData.UserHas(PermissionTo.PublishMovies);
					break;
			}

			return result;
		}

		public static IHtmlContent DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
		{
			if (html == null)
			{
				throw new ArgumentNullException(nameof(html));
			}

			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			var expressionProvider = html.ViewContext.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>();
			var modelExpression = expressionProvider.CreateModelExpression(html.ViewData, expression);

			return new HtmlString(modelExpression.Metadata.Description);
		}

		public static string ToYesNo(this bool val)
		{
			return val ? "Yes" : "No";
		}

		public static bool IsZip(this IFormFile? formFile)
		{
			if (formFile == null)
			{
				return false;
			}

			var acceptableContentTypes = new[]
			{
				"application/x-zip-compressed",
				"application/zip"
			};

			return formFile.FileName.EndsWith(".zip")
				&& acceptableContentTypes.Contains(formFile.ContentType);
		}

		public static bool IsCompressed(this IFormFile? formFile)
		{
			if (formFile == null)
			{
				return false;
			}

			var compressedExtensions = new[]
			{
				".zip", ".gz", "bz2", ".lzma", ".xz"
			};

			var compressedContentTypes = new[]
			{
				"application/x-zip-compressed",
				"application/zip",
				"applicationx-gzip"
			};

			return compressedExtensions.Contains(Path.GetExtension(formFile.FileName))
				|| compressedContentTypes.Contains(formFile.ContentType);
		}

		public static bool LessThanMovieSizeLimit(this IFormFile? formFile)
		{
			if (formFile == null)
			{
				return true;
			}

			return formFile.Length < SiteGlobalConstants.MaximumMovieSize;
		}

		public static bool IsValidImage(this IFormFile? formFile)
		{
			var validImageTypes = new[]
			{
				"image/png", "image/jpeg"
			};

			return validImageTypes.Contains(formFile?.ContentType);
		}
	}
}
