/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Cms
{
	public interface ICommentDataAdapterFactory
	{
		ICommentDataAdapter GetAdapter();

		ICommentDataAdapter GetAdapter(IDataAdapterDependencies dependencies);

		ICommentDataAdapter GetAdapter(IPortalContext portal, RequestContext requestContext);
	}
}
