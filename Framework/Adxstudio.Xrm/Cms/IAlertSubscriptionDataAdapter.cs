/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IAlertSubscriptionDataAdapter
	{
		void CreateAlert(EntityReference user);

		void CreateAlert(EntityReference user, string regardingurl, string regardingid);

		void DeleteAlert(EntityReference user);

		bool HasAlert(EntityReference user);
	}
}
