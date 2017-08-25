/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI.WebControls;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public interface IRegisterClientSideDependenciesProvider
	{
		void Register(WebControl control);
	}
}


