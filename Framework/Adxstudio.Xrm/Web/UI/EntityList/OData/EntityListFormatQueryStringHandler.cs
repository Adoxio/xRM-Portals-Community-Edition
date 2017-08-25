/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Adxstudio.Xrm.Web.UI.EntityList.OData
{
	/// <summary>
	/// Handles $format from query string and change accept header based on it.
	/// $format=json => application/json
	/// $format=jsonverbose => application/json;odata=verbose
	/// $format=atom => application/atom+xml
	/// </summary>
	public class EntityListFormatQueryMessageHandler : DelegatingHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var queryStrings = request.RequestUri.ParseQueryString();
			var format = queryStrings["$format"];

			switch (format)
			{
				case null:
					break;
				case "json":
					request.Headers.Accept.Clear();
					request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
					break;
				case "jsonverbose":
					request.Headers.Accept.Clear();
					request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));
					break;
				case "atom":
					request.Headers.Accept.Clear();
					request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/atom+xml"));
					break;
				default:
					request.Headers.Accept.Clear();
					request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(format));
					break;
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}
