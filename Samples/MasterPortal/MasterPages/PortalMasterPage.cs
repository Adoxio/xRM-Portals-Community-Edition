/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.MasterPages
{
	using System;
	using System.Web.UI.WebControls;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Adxstudio.Xrm;
	using Adxstudio.Xrm.Account;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;

	public class PortalMasterPage : PortalViewMasterPage
	{
		private readonly Lazy<OrganizationServiceContext> _xrmContext;
		private readonly Lazy<string> _languageCode;

		public PortalMasterPage()
		{
			_xrmContext = new Lazy<OrganizationServiceContext>(() => CreateXrmServiceContext());
			_languageCode = new Lazy<string>(GetPortalLanguageCode);
		}

		/// <summary>
		/// Portal language code of the current request.
		/// </summary>
		public string PortalLanguageCode
		{
			get { return _languageCode.Value; }
		}

		/// <summary>
		/// CRM language code of the current request. ex: if the PortalLanguageCode is "en-CA", then the CrmLanguageCode would be "en-US".
		/// </summary>
		public string CrmLanguageCode
		{
			get
			{
				if (string.IsNullOrEmpty(_languageCode.Value))
				{
					return string.Empty;
				}
				string crmLanguageCode;
				if (ContextLanguageInfo.ResolveCultureCode(_languageCode.Value, out crmLanguageCode))
				{
					return crmLanguageCode;
				}
				return _languageCode.Value;
			}
		}

		/// <summary>
		/// A general use <see cref="OrganizationServiceContext"/> for managing entities on the page.
		/// </summary>
		public OrganizationServiceContext XrmContext
		{
			get { return _xrmContext.Value; }
		}

		/// <summary>
		/// The current <see cref="IPortalContext"/> instance.
		/// </summary>
		public IPortalContext Portal
		{
			get { return PortalCrmConfigurationManager.CreatePortalContext(PortalName); }
		}

		/// <summary>
		/// The <see cref="OrganizationServiceContext"/> that is associated with the current <see cref="IPortalContext"/> and used to manage its entities.
		/// </summary>
		/// <remarks>
		/// This <see cref="OrganizationServiceContext"/> instance should be used when querying against the Website, User, or Entity properties.
		/// </remarks>
		public OrganizationServiceContext ServiceContext
		{
			get { return Portal.ServiceContext; }
		}

		/// <summary>
		/// The current adx_website <see cref="Entity"/>.
		/// </summary>
		public Entity Website
		{
			get { return Portal.Website; }
		}

		/// <summary>
		/// The current contact <see cref="Entity"/>.
		/// </summary>
		public Entity Contact
		{
			get { return Portal.User; }
		}

		/// <summary>
		/// The <see cref="Entity"/> representing the current page.
		/// </summary>
		public Entity Entity
		{
			get { return Portal.Entity; }
		}

		/// <summary>
		/// A general use <see cref="IOrganizationService"/> .
		/// </summary>
		public IOrganizationService PortalOrganizationService
		{
			get { return Context.GetOrganizationService(); }
		}

		public void InjectClientsideApmAgent()
		{
			//right now the only Apm system is AppDynamics
			//in the future this might be AppInsights or another engine

			#region AppDynamicsAgent

#if !SelfHosted

			try
			{
				//this code is here to inject the app dynamics js agent code
				//it only does anything if:
				//  1.  the server side agent is turned on AND
				//  2.  the injection feature is turned on (which is a setting on the app dynamics side)
				// in production this might not even be used, just a hook to allow us to use it if we want

				var appdynamicsJsHeader = "AppDynamics_JS_HEADER";
				var contextItems = Context.Items;

				if (contextItems.Contains(appdynamicsJsHeader))
				{
					Response.Write(contextItems[appdynamicsJsHeader]);
				}
			}
			catch (Exception e)
			{
				//unlikely that the above code would ever throw an error under any normal situation
				//whether it works or not, this should never break the app, so just want to catch all the exceptions
				ADXTrace.Instance.TraceError(TraceCategory.Monitoring, $"Unable to inject the AppDynamics Clientside Agent.  Exception: {e}");
			}
#endif

			#endregion
		}

		public bool ForceRegistration
		{
			get
			{
				var siteSetting = this.Context.GetSiteSetting("Profile/ForceSignUp") ?? "false";

				bool value;

				return bool.TryParse(siteSetting, out value) && value;
			}
		}

		protected override void OnInit(EventArgs args)
		{
			base.OnInit(args);

			if (!ForceRegistration)
			{
				return;
			}

			if (!Request.IsAuthenticated || Contact == null)
			{
				return;
			}

			var profilePage = ServiceContext.GetPageBySiteMarkerName(Website, "Profile");

			if (profilePage == null || Entity.ToEntityReference().Equals(profilePage.ToEntityReference()))
			{
				return;
			}

			var profilePath = ServiceContext.GetUrl(profilePage);

			var returnUrl = System.Web.Security.AntiXss.AntiXssEncoder.UrlEncode(Request.Url.PathAndQuery);

			var profileUrl = "{0}{1}{2}={3}".FormatWith(profilePath, profilePath.Contains("?") ? "&" : "?", "ReturnURL", returnUrl);

			if (!ServiceContext.ValidateProfileSuccessfullySaved(Contact))
			{
				Context.RedirectAndEndResponse(profileUrl);
			}
		}

		protected OrganizationServiceContext CreateXrmServiceContext(MergeOption? mergeOption = null)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			if (context != null && mergeOption != null) context.MergeOption = mergeOption.Value;
			return context;
		}

		protected virtual void LinqDataSourceSelecting(object sender, LinqDataSourceSelectEventArgs e)
		{
			e.Arguments.RetrieveTotalRowCount = false;
		}

		private string GetPortalLanguageCode()
		{
			var contextLanguage = this.Context.GetContextLanguageInfo();
			return contextLanguage.ContextLanguage != null ? contextLanguage.ContextLanguage.Code : String.Empty;
		}
	}
}
