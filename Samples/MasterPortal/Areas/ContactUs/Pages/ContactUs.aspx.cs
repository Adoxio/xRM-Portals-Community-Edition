/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.EntityForm;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Site.Pages;

namespace Site.Areas.ContactUs.Pages
{
	public partial class ContactUs : PortalPage
	{
		public static readonly string DefaultSubmitButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Submit_Button_Label_Text"));
		public static readonly string DefaultSubmitButtonBusyText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Default_Modal_Processing_Text"));

		protected void Page_Init(object sender, EventArgs e)
		{
#if TELERIKWEBUI
			const bool captchaEnabled = true;
#else
			const bool captchaEnabled = false;
#endif
			var snippetDataAdapter =
				new SnippetDataAdapter(new PortalContextDataAdapterDependencies(
					PortalCrmConfigurationManager.CreatePortalContext(), requestContext: Context.Request.RequestContext));
			var submitButtonTextSnippet = snippetDataAdapter.Select("ContactUs/Submit");
			var submitButtonText = submitButtonTextSnippet == null || submitButtonTextSnippet.Value == null ||
									string.IsNullOrWhiteSpace(submitButtonTextSnippet.Value.Value as string)
				? DefaultSubmitButtonText
				: (string)submitButtonTextSnippet.Value.Value;
			var submitButtonBusyTextSnippet = snippetDataAdapter.Select("ContactUs/Submit");
			var submitButtonBusyText = submitButtonBusyTextSnippet == null || submitButtonBusyTextSnippet.Value == null ||
									string.IsNullOrWhiteSpace(submitButtonBusyTextSnippet.Value.Value as string)
				? DefaultSubmitButtonBusyText
				: (string)submitButtonBusyTextSnippet.Value.Value;


			FormView.InsertItemTemplate = new ItemTemplate(FormView.ValidationGroup, captchaEnabled, addSubmitButton: true,
				submitButtonCommmandName: "Insert", submitButtonCauseValidation: true, submitButtonText: submitButtonText,
				submitButtonBusyText: submitButtonBusyText, submitButtonCssClass: "btn btn-primary");
		}

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			FormView.Visible = false;
			ConfirmationMessage.Visible = true;
		}
	}
}
