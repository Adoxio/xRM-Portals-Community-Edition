/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.EntityForm;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public class LiquidServerControl : PlaceHolder
	{
		private const string HtmlErrorValidationDiv =
			"<div class='alert alert-block alert-danger'>"
			+ "<p class='text-danger'>"
			+ "<span class='fa fa-exclamation-triangle' aria-hidden='true'>"
			+ "</span> "
			+ "{0}"
			+ " </p>"
			+ "</div>";

		public string Html { get; set; }

		private void AddTemplate(string html)
		{
			var regex = new Regex(@"\<!--\[% (\w+) id:([A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12})? %\]--\>",
				RegexOptions.IgnoreCase);

			Control container = this;

			if (regex.IsMatch(html))
			{
				container = ServerForm(container);
			}

			while (regex.IsMatch(html))
			{
				var match = regex.Match(html);

				var control = match.Groups[1].Value;
				var id = match.Groups[2].Value;
				Guid guid;
				if (Guid.TryParse(id, out guid))
				{
					var splits = html.Split(new[] { match.Value }, StringSplitOptions.RemoveEmptyEntries);
					var preSplit = new LiteralControl(splits[0]);
					container.Controls.Add(preSplit);

					switch (control)
					{
					case "entityform":
						var entityForm = InitEntityForm(guid);
						container.Controls.Add(entityForm);
						break;
					case "webform":
						var webForm = InitWebForm(guid);
						container.Controls.Add(webForm);
						break;
					}

					html = string.Join(match.Value, splits.Skip(1));
				}
			}

			var close = new LiteralControl(html);
			container.Controls.Add(close);
		}

		private EntityForm InitEntityForm(Guid guid)
		{
			var entityForm = new EntityForm
			{
				ID = string.Format("EntityFormControl_{0:N}", guid),
				FormCssClass = "crmEntityFormView",
				PreviousButtonCssClass = "btn btn-default",
				NextButtonCssClass = "btn btn-primary",
				SubmitButtonCssClass = "btn btn-primary",
				ClientIDMode = ClientIDMode.Static
			};

			int languageCode;
			if (TryGetLanguageCode(out languageCode))
			{
				entityForm.LanguageCode = languageCode;
				SetPortalName(entityForm, languageCode);
			}

			entityForm.EntityFormReference = new EntityReference("adx_entityform", guid);
			entityForm.ItemSaved += OnItemSaved;
			return entityForm;
		}
		
		private WebForm InitWebForm(Guid guid)
		{
			var webForm = new WebForm
			{
				ID = string.Format("WebFormControl_{0:N}", guid),
				FormCssClass = "crmEntityFormView",
				PreviousButtonCssClass = "btn btn-default",
				NextButtonCssClass = "btn btn-primary",
				SubmitButtonCssClass = "btn btn-primary",
				ClientIDMode = ClientIDMode.Static
			};

			int languageCode;
			if (TryGetLanguageCode(out languageCode))
			{
				webForm.LanguageCode = languageCode;
				SetPortalName(webForm, languageCode);
			}

			webForm.WebFormReference = new EntityReference("adx_webform", guid);
			webForm.ItemSaved += OnItemSaved;
			return webForm;
		}

		private static bool TryGetLanguageCode(out int languageCode)
		{
			languageCode = 0;

			var languageCodeSetting = HttpContext.Current.Request["languagecode"];

			return !string.IsNullOrWhiteSpace(languageCodeSetting) && int.TryParse(languageCodeSetting, out languageCode);
		}
		
		private static void SetPortalName(EntityForm form, int languageCode)
		{
			var portalName = languageCode.ToString(CultureInfo.InvariantCulture);

			var portals = PortalCrmConfigurationManager.GetPortalCrmSection().Portals;

			if (portals.Count <= 0) return;

			var found = false;

			foreach (var portal in portals)
			{
				var portalContext = portal as PortalContextElement;
				if (portalContext != null && portalContext.Name == portalName)
				{
					found = true;
				}
			}

			if (found)
			{
				form.PortalName = portalName;
			}
		}
		
		private static void SetPortalName(WebForm form, int languageCode)
		{
			var portalName = languageCode.ToString(CultureInfo.InvariantCulture);

			var portals = PortalCrmConfigurationManager.GetPortalCrmSection().Portals;

			if (portals.Count <= 0) return;

			var found = false;

			foreach (var portal in portals)
			{
				var portalContext = portal as PortalContextElement;
				if (portalContext != null && portalContext.Name == portalName)
				{
					found = true;
				}
			}

			if (found)
			{
				form.PortalName = portalName;
			}
		}

		private Control ServerForm(Control container)
		{
			if (Page.Form == null)
			{
				const string formMarkup = @"
<form id=""liquid_form"" runat=""server"">
	<asp:ScriptManager runat=""server"">
		<Scripts>
			<asp:ScriptReference Path=""~/js/jquery.blockUI.js"" />
		</Scripts>
	</asp:ScriptManager>
	<script type=""text/javascript"">
		function entityFormClientValidate() {
			// Custom client side validation. Method is called by the submit button's onclick event.
			// Must return true or false. Returning false will prevent the form from submitting.
			return true;
		}			
	</script>
</form>";
				var aspNetForm = Page.ParseControl(formMarkup);
				if (aspNetForm != null) container.Controls.Add(aspNetForm);

				var pageForm = (HtmlForm)container.FindControl("liquid_form");
				if (pageForm != null) pageForm.Action = HttpContext.Current.Request.Url.PathAndQuery;

				return pageForm ?? container;
			}
			return container;
		}

		protected void OnItemSaved(object sender, EntityFormSavedEventArgs e)
		{
			if (e == null)
			{
				return;
			}
			
			if (e.Exception == null)
			{
				var cs = Page.ClientScript;
				
				if (!cs.IsClientScriptBlockRegistered(GetType(), "EntityFormOnSuccessScript"))
				{
					cs.RegisterClientScriptBlock(GetType(), "EntityFormOnSuccessScript", @"window.parent.postMessage(""Success"", ""*"");", true);
				}
			}
			else if (e.Exception != null && e.Exception.InnerException != null && e.Exception.InnerException.Message != null)
			{
				string errorMessage = TryParseErrorMessage(e.Exception);

				HttpContext.Current.Response.Write(string.Format(HtmlErrorValidationDiv, Page.Server.HtmlEncode(errorMessage)));

				e.ExceptionHandled = true;
			}
		}
		
		protected void OnItemSaved(object sender, WebFormSavedEventArgs e)
		{
			if (e == null)
			{
				return;
			}
			
			if (e.Exception == null)
			{
				var cs = Page.ClientScript;
				
				if (!cs.IsClientScriptBlockRegistered(GetType(), "WebFormOnSuccessScript"))
				{
					cs.RegisterClientScriptBlock(GetType(), "WebFormOnSuccessScript", @"window.parent.postMessage(""Success"", ""*"");", true);
				}
			}
			else if (e.Exception != null && e.Exception.InnerException != null && e.Exception.InnerException.Message != null)
			{
				string errorMessage = TryParseErrorMessage(e.Exception);

				HttpContext.Current.Response.Write(string.Format(HtmlErrorValidationDiv, Page.Server.HtmlEncode(errorMessage)));

				e.ExceptionHandled = true;
			}
		}

		protected override void OnLoad(EventArgs args)
		{
			try
			{
				base.OnLoad(args);
				if (!string.IsNullOrEmpty(Html))
				{
					AddTemplate(Html);
					DataBind();
				}
			}
			catch (Exception e)
			{
			    var guid = WebEventSource.Log.GenericErrorException(e);
			    var errorMessage = string.Format(Resources.ResourceManager.GetString("Generic_Error_Message"), guid);

                HttpContext.Current.Response.Write(string.Format(HtmlErrorValidationDiv, Page.Server.HtmlEncode(errorMessage)));
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			try
			{
				base.Render(writer);
			}
			catch (Exception e)
			{
				var ex = e;
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				writer.Write(HtmlErrorValidationDiv, Page.Server.HtmlEncode(ex.Message));
			}
		}

		private string TryParseErrorMessage(Exception exception)
		{
			string msg = null;
			FaultException<OrganizationServiceFault> fe = exception.InnerException as FaultException<OrganizationServiceFault>;

			if (fe != null)
			{
				// parsing case create/update error message based on web service error codes
				switch (fe.Detail.ErrorCode)
				{
					case -2147204601:
						msg = Resources.ResourceManager.GetString("InvalidEntitlementContacts");
						break;
					case -2147157914:
						msg = Resources.ResourceManager.GetString("InvalidEntitlementForSelectedCustomerOrProduct");
						break;
					case -2147157915:
						msg = Resources.ResourceManager.GetString("InvalidPrimaryContactBasedOnContact");
						break;
					case -2147157916:
						msg = Resources.ResourceManager.GetString("InvalidPrimaryContactBasedOnAccount");
						break;
                    case -2147088887:
                        msg = Resources.ResourceManager.GetString("InvalidEntitlementAssociationToCase");
                        break;

                }
			}
            if (!string.IsNullOrEmpty(msg))
            {
                return msg;
            }

            var guid = WebEventSource.Log.GenericErrorException(exception);
            msg = string.Format(Resources.ResourceManager.GetString("Generic_Error_Message"), guid);

            return msg;
		}
	}
}
