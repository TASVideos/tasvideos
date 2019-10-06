using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
// Core 3 TODO
//using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

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
					result ^= viewContext.HttpContext.User.Identity.IsAuthenticated;
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
					result ^= viewContext.HttpContext.User.Identity
						.IsAuthenticated; // Let's assume every user can have a homepage automatically
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
	
		// ReSharper disable once UnusedMember.Global (Used in Node.cs with string building)
		public static bool WikiCondition(this IHtmlHelper html, string condition)
		{
			return WikiCondition(html.ViewContext, condition);
		}

		// https://stackoverflow.com/questions/6578495/how-do-i-display-the-displayattribute-description-attribute-value
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

			// Core 3 TODO
			//var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
			//if (modelExplorer == null)
			//{
			//	throw new InvalidOperationException($"Failed to get model explorer for {ExpressionHelper.GetExpressionText(expression)}");
			//}
			
			//return new HtmlString(modelExplorer.Metadata.Description);
			return new HtmlString("");
		}

		public static string ToYesNo(this bool val)
		{
			return val ? "Yes" : "No";
		}

		public static bool IsZip(this IFormFile formFile)
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

		public static bool IsCompressed(this IFormFile formFile)
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
		
		public static bool LessThanMovieSizeLimit(this IFormFile formFile)
		{
			if (formFile == null)
			{
				return true;
			}

			return formFile.Length < SiteGlobalConstants.MaximumMovieSize;
		}
	}
}
