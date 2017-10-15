/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using System.Globalization;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public class CommentCreatorFormView : CrmEntityFormView
	{
		protected override void OnInit(EventArgs e)
		{
			//var context = CrmConfigurationManager.CreateContext(ContextName, true);

			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			var context = portal.ServiceContext;

			var entity = portal.Entity;

			var commentAdapterFactory = new CommentDataAdapterFactory(entity.ToEntityReference());

			var dataAdapter = commentAdapterFactory.GetAdapter();

			if (dataAdapter == null)
			{
				Visible = false;

				return;
			}

			var logicalName = dataAdapter.GetCommentLogicalName();

			EntityName = logicalName;

			EntityMetadata = context.RetrieveEntity(EntityName, EntityFilters.Attributes);
		}
	}
}
