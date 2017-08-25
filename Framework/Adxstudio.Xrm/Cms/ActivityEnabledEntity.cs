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
	/// <summary>
	/// Every Entity Object that is to be used by data adapters should inherit from this if possible
	/// </summary>
	public class ActivityEnabledEntity : ISubscribable
	{
		public Entity Entity { get; set; }

		public EntityReference EntityReference
		{
			get { return Entity.ToEntityReference(); }
		}
		
	}
}
