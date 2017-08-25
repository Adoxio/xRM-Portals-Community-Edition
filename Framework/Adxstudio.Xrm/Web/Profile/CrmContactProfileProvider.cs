/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;

namespace Adxstudio.Xrm.Web.Profile
{
	public class CrmContactProfileProvider : CrmProfileProvider
	{
		public override void Initialize(string name, NameValueCollection config)
		{
			config["attributeMapStateCode"] = config["attributeMapStateCode"] ?? "statecode";

			config["attributeMapIsAnonymous"] = config["attributeMapIsAnonymous"] ?? "adx_profileisanonymous";

			config["attributeMapLastActivityDate"] = config["attributeMapLastActivityDate"] ?? "adx_profilelastactivity";

			config["attributeMapLastUpdatedDate"] = config["attributeMapLastUpdatedDate"] ?? "modifiedon";

			config["attributeMapUsername"] = config["attributeMapUsername"] ?? "adx_username";

			config["profileEntityName"] = config["profileEntityName"] ?? "contact";

			base.Initialize(name, config);
		}
	}
}
