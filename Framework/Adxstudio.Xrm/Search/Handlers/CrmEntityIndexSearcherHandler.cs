/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Microsoft.Xrm.Portal.Configuration;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.Search.Handlers
{
	public class CrmEntityIndexSearcherHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "application/json";

			try
			{
				var results = Search(context.Request);

				var json = results.SerializeByJson(new Type[] { });

				context.Response.Write(json);
			}
			catch (Exception e)
			{              
				var json = (new ExceptionData(e)).SerializeByJson(new Type[] { });

				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.Write(json);
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}

		private static ICrmEntitySearchResultPage Search(HttpRequest request)
		{
			using (var searcher = SearchManager.Provider.GetIndexSearcher())
			{
				var query = GetRequiredParam(request, "query");
				var page = GetRequiredIntParam(request, "page");
				var pageSize = GetRequiredIntParam(request, "pageSize");
				var contextLanguage = HttpContext.Current.GetContextLanguageInfo();

				var logicalNames = (request.QueryString["logicalNames"] ?? string.Empty)
					.Split(',')
					.Select(name => name.Trim())
					.Where(name => !string.IsNullOrEmpty(name));

				var rawResults = searcher.Search(new CrmEntityQuery(query, page, pageSize, logicalNames, contextLanguage.ContextLanguage, contextLanguage.IsCrmMultiLanguageEnabled));

				var results = rawResults.Select(result => new Services.CrmEntitySearchResult(result.EntityLogicalName, result.EntityID, result.Title, result.Url, result.Fragment, result.ResultNumber, result.Score, result.ExtendedAttributes));

				return new Services.CrmEntitySearchResultPage(results, rawResults.ApproximateTotalHits, rawResults.PageNumber, rawResults.PageSize);
			}
		}

		private static string GetRequiredParam(HttpRequest request, string name)
		{
			var value = request.QueryString[name];

			if (value == null)
			{
				throw new InvalidOperationException("The querystring parameter {0} can not be null.".FormatWith(name));
			}

			return value;
		}

		private static int GetRequiredIntParam(HttpRequest request, string name)
		{
			var stringValue = GetRequiredParam(request, name);

			int value;

			if (int.TryParse(stringValue, out value))
			{
				return value;
			}

			throw new InvalidOperationException("The querystring parameter {0} must be an integer.".FormatWith(name));
		}
	}
}
