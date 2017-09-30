/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Handle exception responses and error response codes in a way that is compatible with the
	/// jquery.form plugin and IE 8-9 iframe-based file uploads.
	/// </summary>
	public class AjaxFormStatusResponseAttribute : ActionFilterAttribute, IExceptionFilter
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var httpStatusResult = filterContext.Result as HttpStatusCodeResult;

			if (httpStatusResult == null || httpStatusResult.StatusCode < 400)
			{
				base.OnActionExecuted(filterContext);

				return;
			}

			// We can't allow 403 Forbidden as a status code, as IE 8-9 file uploads will be done
			// through a hidden iframe instead of an AJAX request, and will thus trigger the portal's
			// authentication redirect if we don't send an alternate code.
			var statusCode = httpStatusResult.StatusCode == (int)HttpStatusCode.Forbidden
				? (int)HttpStatusCode.BadRequest
				: httpStatusResult.StatusCode;

			filterContext.Result = GetErrorResult(filterContext, statusCode, httpStatusResult.StatusDescription);

			UpdateResponse(filterContext, statusCode, httpStatusResult.StatusDescription);
		}

		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext.ExceptionHandled || filterContext.Exception == null)
			{
				return;
			}

			var guid = WebEventSource.Log.UnhandledException(filterContext.Exception);
			var message = string.Format(ResourceManager.GetString("Page_500_Something_Went_Wrong_Text"), guid);

			filterContext.Result = GetErrorResult(filterContext, (int)HttpStatusCode.InternalServerError, message);
			filterContext.ExceptionHandled = true;

			UpdateResponse(filterContext, (int)HttpStatusCode.InternalServerError, message);
		}

		private static ActionResult GetErrorResult(ControllerContext filterContext, int statusCode, string statusDescription)
		{
			return new ContentResult
			{
				ContentType = "text/html",
				Content = @"<!DOCTYPE html>
<html>
	<head>
		<title>({0}) {1}</title>
	</head>
	<body status=""{0}"" statusText=""{1}"">
		{1}
	</body>
</html>".FormatWith(statusCode, HttpUtility.HtmlEncode(statusDescription ?? string.Empty))
			};
		}

		private static void UpdateResponse(ControllerContext filterContext, int statusCode, string statusDescription)
		{
			filterContext.HttpContext.Response.Clear();
			filterContext.HttpContext.Response.StatusCode = statusCode;
			filterContext.HttpContext.Response.StatusDescription = statusDescription;
			filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
		}
	}
}
