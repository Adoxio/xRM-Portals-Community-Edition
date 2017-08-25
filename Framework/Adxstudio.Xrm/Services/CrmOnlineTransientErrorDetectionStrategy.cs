/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Practices.TransientFaultHandling;

namespace Adxstudio.Xrm.Services
{
	public class CrmOnlineTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		public virtual bool IsTransient(Exception ex)
		{
			return false;
		}
	}
}
