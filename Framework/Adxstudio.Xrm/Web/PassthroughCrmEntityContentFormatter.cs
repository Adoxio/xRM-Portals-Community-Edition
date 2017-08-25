/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web
{
	public class PassthroughCrmEntityContentFormatter : ICrmEntityContentFormatter
	{
		public string Format(string content, Entity entity, object context)
		{
			return content;
		}
	}
}
