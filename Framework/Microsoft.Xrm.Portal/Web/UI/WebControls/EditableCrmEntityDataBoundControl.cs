/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public abstract class EditableCrmEntityDataBoundControl : CrmEntityDataBoundControl, IEditableCrmEntityControl
	{
		private bool _editable = true;

		/// <summary>
		/// Gets or sets the base URI of the data service to be used for front-side editing functionality
		/// provided by this control. Set to use a data service other than the system global/default service.
		/// </summary>
		public virtual string CmsServiceBaseUri { get; set; }

		/// <summary>
		/// Gets or sets a Boolean value indication whether or not this property value will be inline editable
		/// (provided the user has edit permission, and no other properties have been set on this control which
		/// disable inline editing support).
		/// </summary>
		public virtual bool Editable
		{
			get { return _editable; }
			set { _editable = value; }
		}

		protected virtual bool HasEditPermission(Entity entity)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var context = portal.ServiceContext;

			entity = context.MergeClone(entity);

			return securityProvider.TryAssert(context, entity, CrmEntityRight.Change);
		}
	}
}


