/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc
{
	public class JContainerResult : ActionResult
	{
		private readonly JContainer _json;

		public JContainerResult(JContainer json)
		{
			if (json == null) throw new ArgumentNullException("json");

			_json = json;
		}

		public Encoding ContentEncoding { get; set; }

		public string ContentType { get; set; }

		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			var response = context.HttpContext.Response;

			response.ContentType = string.IsNullOrEmpty(ContentType)
				? "application/json"
				: ContentType;

			if (ContentEncoding != null)
			{
				response.ContentEncoding = ContentEncoding;
			}

			response.Write(_json.ToString());
		}
	}
}
