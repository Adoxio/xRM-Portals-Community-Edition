/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adxstudio.Xrm.Core.Flighting
{
	/// <summary>
	/// Does the FCB check against the data store.
	/// </summary>
	public class FeatureControlChecks : IFeatureControlCheck
	{

		public IFeatureDetailContainer FeatureDetails { get; set; }

		public FeatureControlChecks()
		{
			FeatureDetails = new FeatureDetailContainer();
		}

		/// <summary>
		/// Current implementation of feature check which checks against a static config back store.
		/// </summary>
		/// <param name="featureName"></param>
		/// <returns></returns>
		public bool? IsFeatureEnabled(string featureName, Guid organizationId, Guid portalId)
		{
			bool? isFeatureEnabled = null;

			if (FeatureDetails != null && FeatureDetails.Features != null && FeatureDetails.Features.ContainsKey(featureName))
			{
				var feature = FeatureDetails.Features[featureName];
				if (feature != null)
				{
					isFeatureEnabled = feature.IsEnabled;
				}
			}

			return isFeatureEnabled;
		}
	}
}
