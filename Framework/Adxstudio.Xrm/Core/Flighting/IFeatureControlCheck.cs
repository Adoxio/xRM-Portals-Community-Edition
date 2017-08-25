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
	public interface IFeatureControlCheck
	{
		 bool? IsFeatureEnabled(string featureName, Guid organizationId, Guid portalId);
	}
}
