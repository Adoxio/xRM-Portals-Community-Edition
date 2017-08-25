/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;

namespace Adxstudio.Xrm.Web.Profile
{
	public class CrmUserProfileProvider : CrmProfileProvider
	{
		public override void Initialize(string name, NameValueCollection config)
		{
			config["attributeMapIsDisabled"] = config["attributeMapIsDisabled"] ?? "isdisabled";

			config["attributeMapIsAnonymous"] = config["attributeMapIsAnonymous"] ?? "adx_profileisanonymous";

			config["attributeMapLastActivityDate"] = config["attributeMapLastActivityDate"] ?? "adx_profilelastactivity";

			config["attributeMapLastUpdatedDate"] = config["attributeMapLastUpdatedDate"] ?? "modifiedon";

			config["attributeMapUsername"] = config["attributeMapUsername"] ?? "domainname";

			config["profileEntityName"] = config["profileEntityName"] ?? "systemuser";

			base.Initialize(name, config);
		}
	}
}
