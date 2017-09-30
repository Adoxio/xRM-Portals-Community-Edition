/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc
{
	public class JObjectResult : JContainerResult
	{
		public JObjectResult(JObject json) : base(json) { }
	}
}
