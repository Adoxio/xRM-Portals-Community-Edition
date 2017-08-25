/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc
{
	public interface IPortalViewAttribute
	{
		string Description { get; }

		bool Editable { get; }

		EntityReference EntityReference { get; }

		string LogicalName { get; }

		object Value { get; }
	}
}
