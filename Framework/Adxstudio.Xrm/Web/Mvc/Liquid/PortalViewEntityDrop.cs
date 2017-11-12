/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebForms;
using DotLiquid;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PortalViewEntityDrop : EntityDrop, IEditable
	{
		public PortalViewEntityDrop(IPortalLiquidContext portalLiquidContext, IPortalViewEntity viewEntity) : base(portalLiquidContext)
		{
			if (viewEntity == null) throw new ArgumentNullException("viewEntity");

			ViewEntity = viewEntity;
		}

		public virtual bool Editable
		{
			get { return ViewEntity.Editable; }
		}

		public override string Url
		{
			get { return ViewEntity.Url; }
		}

		protected override Entity Entity
		{
			get { return FullEntity; }
		}

		protected override EntityReference EntityReference
		{
			get { return ViewEntity.EntityReference; }
		}

		protected IPortalViewEntity ViewEntity { get; private set; }

		public override object BeforeMethod(string method)
		{
			var viewAttribute = ViewEntity.GetAttribute(method);

			if (viewAttribute != null && viewAttribute.Value != null)
			{
				//This is needed to localize user Full Name respectfully to users locale
				if (method == "fullname")
				{
					var fullName = GetLocalizedFullName();

					return TransformAttributeValueForLiquid(LogicalName, method, fullName);
				}
				return TransformAttributeValueForLiquid(LogicalName, method, viewAttribute.Value);
			}

			return base.BeforeMethod(method);
		}

		public virtual string GetEditable(Context context, EditableOptions options)
		{
			var html = Html.EntityEditingMetadata(ViewEntity);

			return html == null ? null : html.ToString();
		}

		private string GetLocalizedFullName()
		{
			var firstNameAttribute = ViewEntity.GetAttribute("firstname");
			var lastNameAttribute = ViewEntity.GetAttribute("lastname");

			var firstName = string.Empty;
			var lastName = string.Empty;
			if (firstNameAttribute != null && firstNameAttribute.Value != null)
			{
				firstName = firstNameAttribute.Value.ToString();
			}
			if (lastNameAttribute != null && lastNameAttribute.Value != null)
			{
				lastName = lastNameAttribute.Value.ToString();
			}

			return Localization.LocalizeFullName(firstName, lastName);
		}
	}
}
