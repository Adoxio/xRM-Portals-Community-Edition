/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// A UserControl that is to be loaded by a Web Form must be based on this custom UserControl derived class.
	/// </summary>
	public class WebFormUserControl : UserControl
	{
		protected internal Guid PreviousStepEntityID;
		protected internal string PreviousStepEntityLogicalName;
		protected internal string PreviousStepEntityPrimaryKeyLogicalName;
		protected internal Guid CurrentStepEntityID;
		protected internal string CurrentStepEntityLogicalName;
		protected internal string CurrentStepEntityPrimaryKeyLogicalName;
		protected internal int LanguageCode;
		protected internal string PortalName;
		protected internal string ValidationGroup;
		protected internal string PostBackUrl;
		protected internal bool SetEntityReference;
		protected internal string EntityReferenceRelationshipName;
		protected internal string EntityReferenceTargetEntityName;
		protected internal string EntityReferenceTargetEntityPrimaryKeyName;
		protected internal Guid EntityReferenceTargetEntityID;
		protected internal string LoadEventKeyName;
		protected internal WebForm WebForm {
			get
			{
				var webform = Parent.Parent as WebForm;
				if (webform == null)
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                    return null;
				}
				return webform;
			}
		}
		protected internal IEnumerable<Entity> WebFormMetadata;

		protected internal void SetAttributeValuesAndSave(OrganizationServiceContext context, Entity entity)
		{
			var webform = Parent.Parent as WebForm;
			if (webform == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                return;
			}
			webform.SetAttributeValuesAndSave(context, entity);
		}

		protected internal bool TrySetAttributeValuesFromMetadata(OrganizationServiceContext context, ref Entity entity)
		{
			var webform = Parent.Parent as WebForm;
			if (webform == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                return false;
			}
			return webform.TrySetAttributeValuesFromMetadata(context, ref entity);
		}

		protected internal void MoveToPreviousStep()
		{
			var webform = Parent.Parent as WebForm;
			if (webform == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                return;
			}
			webform.MoveToPreviousStep();
		}

		protected internal void MoveToNextStep()
		{
			MoveToNextStep(new Guid());
		}

		protected internal void MoveToNextStep(Guid entityID)
		{
			var webform = Parent.Parent as WebForm;
			if (webform == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                return;
			}
			webform.MoveToNextStep(entityID);
		}

		protected internal void MoveToNextStep(WebFormEntitySourceDefinition entityDefinition)
		{
			var webform = Parent.Parent as WebForm;
			if (webform == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                return;
			}
			webform.MoveToNextStep(entityDefinition);
		}

		protected internal void UpdateEntityDefinition(WebFormEntitySourceDefinition entityDefinition)
		{
			var webform = Parent.Parent as WebForm;
			if (webform == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Parent WebForm could not be resolved.");
                return;
			}
			webform.UpdateEntityDefinition(entityDefinition);
		}

		protected internal virtual void OnMovePrevious(object sender, WebFormMovePreviousEventArgs e)
		{
			// Set e.Cancel = true; to prevent web form from proceeding to the previous step.
			// Set e.PreviousStepEntityLogicalName, e.PreviousStepEntityPrimaryKeyLogicalName, e.PreviousStepEntityID with the values of the record.
		}

		protected internal virtual void OnSubmit(object sender, WebFormSubmitEventArgs e)
		{
			// Set e.Cancel = true; to prevent web form from proceeding.
			// Set e.EntityLogicalName, e.EntityPrimaryKeyLogicalName, e.EntityID with the values of the created record.
		}
	}
}
