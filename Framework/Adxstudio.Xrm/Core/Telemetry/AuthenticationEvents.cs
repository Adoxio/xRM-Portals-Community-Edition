/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adxstudio.Xrm.Core.Telemetry
{
	[EventSource(Guid = "55A38D29-FABA-41DD-818C-AF2089CAA683", Name = "AuthenticationEvents")]
	public sealed class AuthenticationEvents : EventSource
	{
		private readonly static Lazy<AuthenticationEvents> _instance = new Lazy<AuthenticationEvents>();

		public static AuthenticationEvents Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		private enum EventNames
		{
			AuthenticationProvider = 1
		}

		[Event((int)EventNames.AuthenticationProvider)]
		public void LogAuthenticationProvider(Guid organizationId, Guid portalId, string authenticationProvider)
		{
			this.WriteEvent((int)EventNames.AuthenticationProvider, organizationId, portalId, authenticationProvider);
		}
	}
}
