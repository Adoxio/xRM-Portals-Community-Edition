/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Diagnostics.Trace;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	public class CrmQuickFormControlTemplate : CrmQuickFormCellTemplate
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="contextName">Name of the context the portal binds to</param>
		/// <param name="bindings">Dictionary of the cell bindings</param>
		public CrmQuickFormControlTemplate(FormXmlCellMetadata metadata, string contextName, IDictionary<string, CellBinding> bindings)
			: base(metadata)
		{
			ContextName = contextName;
			Bindings = bindings;
		}

		/// <summary>
		/// Name of the context the portal binds to
		/// </summary>
		protected string ContextName { get; set; }

		/// <summary>
		/// Dictionary of the cell bindings
		/// </summary>
		protected IDictionary<string, CellBinding> Bindings { get; private set; }

		/// <summary>
		/// Control instantiation
		/// </summary>
		/// <param name="container"></param>
		protected override void InstantiateControlIn(HtmlControl container)
		{
			if (!Metadata.IsQuickForm || Metadata.QuickForm == null) return;

			Bindings[Metadata.ControlID + "CrmEntityId"] = new CellBinding
			{
				Get = () => null,
				Set = obj =>
				{
					var id = obj.ToString();
					Guid entityId;

					if (!Guid.TryParse(id, out entityId))
					{
						return;
					}

					CreateControls(container, entityId);
				}
			};

			if (!container.Page.IsPostBack)
			{
				return;
			}

			// On PostBack no databinding occurs so get the id from the viewstate stored on the CrmEntityFormView control.

			var crmEntityId = Metadata.FormView.CrmEntityId;

			if (crmEntityId == null)
			{
				return;
			}

			CreateControls(container, (Guid)crmEntityId);
		}

		protected void CreateControls(Control container, Guid entityId)
		{
			// Add a placeholder element with the control ID that will always be present, so
			// that any potential cell label will still have an control to associate with.
			var placeholder = new PlaceHolder { ID = Metadata.ControlID };

			container.Controls.Add(placeholder);

			var context = CrmConfigurationManager.CreateContext(ContextName, true);
			var quickForm = Metadata.QuickForm.QuickFormIds.FirstOrDefault();
			
			if (quickForm == null) return;

			var filterExpression = new FilterExpression();
			filterExpression.Conditions.Add(new ConditionExpression("formid", ConditionOperator.Equal, quickForm.FormId));

			var systemForm = Xrm.Metadata.OrganizationServiceContextExtensions.GetMultipleSystemFormsWithAllLabels(filterExpression, context).FirstOrDefault();

			if (systemForm == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Quick Form: A form with ID='{0}' could not be found.".FormatWith(quickForm.FormId)));

                return;
			}

			var formName = systemForm.GetAttributeValue<string>("name");

			if (string.IsNullOrWhiteSpace(formName))
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Quick Form: Form with ID='{0}' does not have a name.".FormatWith(quickForm.FormId)));

                return;
			}

			var templatePath = GetPortalQuickFormTemplatePath(ContextName);

			if (string.IsNullOrWhiteSpace(templatePath))
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to retrieve Quick Form template path.");

                return;
			}

			var iframe = new HtmlIframe { ID = Metadata.ControlID, Src = "about:blank" };

			iframe.Attributes.Add("class", "quickform");
			iframe.Attributes.Add("data-path", templatePath);
			iframe.Attributes.Add("data-controlid", Metadata.ControlID);
			iframe.Attributes.Add("data-formname", formName);
			iframe.Attributes.Add("data-lookup-element", Metadata.DataFieldName);
			
			container.Controls.Remove(placeholder);
			container.Controls.Add(iframe);

			var primaryKeyAttributeName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, quickForm.EntityName);
		
			if (string.IsNullOrWhiteSpace(primaryKeyAttributeName))
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error retrieving the Primary Key Attribute Name for '{0}'.", EntityNamePrivacy.GetEntityName(quickForm.EntityName)));

                return;
			}

			if (string.IsNullOrWhiteSpace(Metadata.DataFieldName))
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Quick Form: datafieldname is null for QuickForm with ID='{0}'", quickForm.FormId));

                return;
			}

			var formEntity = context.CreateQuery(Metadata.TargetEntityName).FirstOrDefault(e => e.GetAttributeValue<Guid>(Metadata.TargetEntityPrimaryKeyName) == entityId);

			if (formEntity == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to retrieve entity record with ID='{0}'", entityId));

                return;
			}

			var quickFormEntityReference = formEntity.GetAttributeValue<EntityReference>(Metadata.DataFieldName);

			if (quickFormEntityReference == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Attribute on entity record with ID='{0}' is null. Quick Form not loaded.", entityId));

				return;
			}

			var src = templatePath;

			src = src.AppendQueryString("entityid", quickFormEntityReference.Id.ToString());
			src = src.AppendQueryString("entityname", quickForm.EntityName);
			src = src.AppendQueryString("entityprimarykeyname", primaryKeyAttributeName);
			src = src.AppendQueryString("formname", formName);
			src = src.AppendQueryString("controlid", Metadata.ControlID);

			iframe.Src = src;
		}

		private static string GetPortalQuickFormTemplatePath(string portalName)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
			var website = portal.Website;

			var http = HttpContext.Current;

			if (http == null) return null;

			var requestContext = new RequestContext(new HttpContextWrapper(http), new RouteData());

			VirtualPathData virtualPath;

			if (website == null)
			{
				virtualPath = RouteTable.Routes.GetVirtualPath(requestContext, "PortalQuickFormTemplatePath", new RouteValueDictionary());
			}
			else
			{
				virtualPath = RouteTable.Routes.GetVirtualPath(requestContext, "PortalQuickFormTemplatePath", new RouteValueDictionary
				{
					{ "__portalScopeId__", website.Id }
				});
			}

			var path = virtualPath == null ? null : VirtualPathUtility.ToAbsolute(virtualPath.VirtualPath);
			return path;
		}
	}
}
