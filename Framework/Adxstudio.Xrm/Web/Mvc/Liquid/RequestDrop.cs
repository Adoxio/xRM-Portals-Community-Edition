/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class RequestDrop : PortalDrop
	{
		private readonly Lazy<Hash> _params;
		private readonly HttpRequestBase _request;

		public RequestDrop(IPortalLiquidContext portalLiquidContext, HttpRequestBase request) : base(portalLiquidContext)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));

			_request = request;

			if (_request.Url != null)
			{
				Path = _request.Url.AbsolutePath;
				PathAndQuery = _request.Url.PathAndQuery;
				Query = _request.Url.Query;
				Url = _request.Url.ToString();
			}

			RawUrl = HttpUtility.UrlEncode(_request.RawUrl); //For compatibility with previous versions
			RawUrlEncode = HttpUtility.UrlEncode(_request.RawUrl);
			_params = new Lazy<Hash>(GetParams, LazyThreadSafetyMode.None);
		}

		public string AntiForgeryToken
		{
			get { return Html.AntiForgeryToken().ToString(); }
		}

		public Hash Params
		{
			get { return _params.Value; }
		}

		public string Path { get; private set; }

		public string PathAndQuery { get; private set; }

		public string Query { get; private set; }

		public string RawUrl { get; private set; }

		public string RawUrlEncode { get; private set; }

		public string Url { get; private set; }

		public static RequestDrop FromHtmlHelper(IPortalLiquidContext portalLiquidContext, HtmlHelper html)
		{
			if (portalLiquidContext == null) throw new ArgumentNullException(nameof(portalLiquidContext));
			if (html == null) throw new ArgumentNullException(nameof(html));

			if (html.ViewContext == null || html.ViewContext.HttpContext == null || html.ViewContext.HttpContext.Request == null)
			{
				return null;
			}

			return new RequestDrop(portalLiquidContext, html.ViewContext.HttpContext.Request);
		}

		private Hash GetParams()
		{
			var @params = new Hash();

			var requestParams = _request.Params;

			if (requestParams != null && requestParams.HasKeys())
			{
				var items = requestParams.AllKeys
					.Where(key => !string.IsNullOrEmpty(key))
					.SelectMany(key => requestParams.GetValues(key) ?? Enumerable.Empty<string>(), (key, values) => new { key, values });

				foreach (var item in items)
				{
					@params[item.key] = item.values;
				}
			}
			
			if (Html.ViewContext != null && Html.ViewContext.ViewData != null)
			{
				@params.Merge(Html.ViewContext.ViewData);
			}

			if (Html.ViewData != null)
			{
				@params.Merge(Html.ViewData);
			}

			return @params;
		}
	}
}
