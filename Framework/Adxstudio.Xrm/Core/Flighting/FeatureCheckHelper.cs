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
	public static class FeatureCheckHelper
	{
		/// <summary>
		/// Static constructor instantiating the right FeatureControlCheck implementation
		/// </summary>
		static FeatureCheckHelper()
		{
			Current = new FeatureControlChecks();
		}

		public static IFeatureControlCheck Current { get; set; }

		/// <summary>
		/// Helps you identify if the feature is enabled
		/// </summary>
		/// <param name="featureName"></param>
		/// <returns></returns>
		public static bool IsFeatureEnabled(string featureName)
		{
			var organizationId = Guid.Empty; // TODO figure how to get the Organization Id that portal is connected to here. 
			var portalId = Guid.Empty; // TODO figure how to get the id which uniquely identifies this portal.

			var isFeatureEnabled = Current.IsFeatureEnabled(featureName, organizationId, portalId);
			return isFeatureEnabled.GetValueOrDefault();
		}
	}
}
