/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;

namespace Adxstudio.Xrm.Web.Handlers
{
	[Serializable]
	public class CmsEntityServiceException : Exception
	{
		public CmsEntityServiceException(HttpStatusCode statusCode, string message) : base(message)
		{
			StatusCode = statusCode;
		}

		public CmsEntityServiceException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
		{
			StatusCode = statusCode;
		}

		public HttpStatusCode StatusCode { get; private set; }
	}
}
