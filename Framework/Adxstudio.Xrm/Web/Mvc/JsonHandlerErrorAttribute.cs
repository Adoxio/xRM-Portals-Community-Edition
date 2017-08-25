/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;
using System.Web.Mvc;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// An <see cref="IExceptionFilter"/> for returning errors.
	/// </summary>
	public class JsonHandlerErrorAttribute : HandleErrorAttribute
	{
		public override void OnException(ExceptionContext filterContext)
		{
			if (!filterContext.ExceptionHandled)
			{
				filterContext.ExceptionHandled = true;
				filterContext.HttpContext.Response.Clear();
				filterContext.HttpContext.Response.StatusCode = 500;

				var guid = WebEventSource.Log.UnhandledException(filterContext.Exception);
				var message = string.Format(ResourceManager.GetString("Page_500_Something_Went_Wrong_Text"), guid);

				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError, message);
				
				return;
			}

			base.OnException(filterContext);
		}
	}
}
