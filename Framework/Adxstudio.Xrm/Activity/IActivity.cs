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

namespace Adxstudio.Xrm.Activity
{
	public interface IActivity
	{
		Entity Entity { get; set; }
		EntityReference Regarding { get; set; }
	}

	public enum StateCode
	{
		Open = 0,
		Completed = 1,
		Canceled = 2,
		Scheduled = 3
	}
}
