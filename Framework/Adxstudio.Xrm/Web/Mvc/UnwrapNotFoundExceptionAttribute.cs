/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Runtime.ExceptionServices;
	using System.Web;
	using System.Web.Mvc;

	/// <summary>
	/// Finds and rethrows the original 404 <see cref="HttpException"/> that happened during a request.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class UnwrapNotFoundExceptionAttribute : FilterAttribute, IExceptionFilter
	{
		/// <summary>
		/// Invoked on exception that happens during the processing of current request in controller action.
		/// </summary>
		/// <param name="filterContext">Exception context of the current request.</param>
		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext.ExceptionHandled)
			{
				return;
			}

			var topLevelException = filterContext.Exception as HttpException;

			// No need to unwrap top-level 404 exceptions
			if (topLevelException == null || IsNotFoundException(topLevelException))
			{
				return;
			}

			var httpException = FindOriginalHttpException(topLevelException);
			if (httpException != null && IsNotFoundException(httpException))
			{
				filterContext.RequestContext.HttpContext.Response.Clear();

				// Rethrow the original 404 exception preserving stack trace and other information
				ExceptionDispatchInfo.Capture(httpException).Throw();
			}
		}

		/// <summary>
		/// Finds the first <see cref="HttpException"/> that started the chain
		/// </summary>
		/// <param name="ex">Instance of <see cref="Exception"/> that needs to be analyzed.</param>
		/// <returns>The first <see cref="HttpException"/></returns>
		private static HttpException FindOriginalHttpException(Exception ex)
		{
			return GetAllHttpExceptions(ex).LastOrDefault();
		}

		/// <summary>
		/// Finds all nested exceptions of type <see cref="HttpException"/> contained within a certain <see cref="Exception"/>.
		/// </summary>
		/// <param name="ex">Instance of <see cref="Exception"/> that needs to be analyzed.</param>
		/// <returns>A collection of all <see cref="HttpException"/> instaces found within the initial <see cref="Exception"/>.</returns>
		private static IEnumerable<HttpException> GetAllHttpExceptions(Exception ex)
		{
			while (true)
			{
				var exception = ex as HttpException;
				if (exception != null)
				{
					yield return exception;
				}
				if (ex.InnerException == null)
				{
					yield break;
				}
				ex = ex.InnerException;
			}
		}

		/// <summary>
		/// Determines if given <see cref="HttpException"/> represents a 404 Not Found error.
		/// </summary>
		/// <param name="ex">Exception instance.</param>
		/// <returns>True if given <see cref="HttpException"/> represents a 404 Not Found error, False otherwise.</returns>
		private static bool IsNotFoundException(HttpException ex)
		{
			return ex.GetHttpCode() == (int)HttpStatusCode.NotFound;
		}
	}
}
