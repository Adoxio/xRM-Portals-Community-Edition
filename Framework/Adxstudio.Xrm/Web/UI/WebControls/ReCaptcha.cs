/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Telerik's RadCaptcha control provides two major strategies for protection against automated form submissions:
	/// 1. Image with modified symbols.They are displayed in a form, and the user is required to input the symbols in 
	///    a textbox. If the input is correct, the control validates that the user is not a robot because it is not yet 
	///    possible for robots to identify distorted symbols.This is the most secure method to protect from comment spam.
	/// 2. Automatic Robots Discovery � this strategy uses predefined rules which decide whether the input comes from a 
	///    robot or not. This strategy is not 100% secure and some sophisticated robots may pass it. The Sitefinity 
	///    administrator is allowed to decide which of the predefined rules to use.
	///
	/// http://docs.telerik.com/devtools/aspnet-ajax/controls/captcha/overview
	/// </summary>
	public static class RadCaptcha
	{
		private static readonly string DefaultErrorMessage = ResourceManager.GetString("CaptchaDefaultErrorMessage");
		private const string DefaultControlID = "captchaControl";
		private const string AudioFilesPath = @"\Content\RadCaptcha";

#if TELERIKWEBUI

		private const string RadCaptchaScript = "~/xrm-adx/js/radcaptcha.js";

#endif

		/// <summary>
		/// The error message to be displayed when the captcha control validation is invalid.
		/// </summary>
		public static string ErrorMessage { get; set; }

		static RadCaptcha()
		{
#if TELERIKWEBUI
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			var site = portalContext.Website;

			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var website = context.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == site.Id);

			if (website == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_website with the ID {0}.", site.Id));
			}

			var errorMessage = context.GetSiteSettingValueByName(website, "RadCaptcha/ErrorMessage");

			ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? DefaultErrorMessage : errorMessage;
#endif
		}

		/// <summary>
		/// Adds captcha to the container's control collection.
		/// </summary>
		/// <param name="container">The control that will contain the Captcha.</param>
		/// <param name="controlID">The id of the CaptchaControl.</param>
		/// <param name="validationGroup">The name of the group of controls that is validated on validation.</param>
		/// <param name="renderScript">bool tell if scripts need to rendered or not.</param>
		/// <returns>IValidator control which the rad captcha control</returns>
		public static IValidator RenderCaptcha(Control container, string controlID, string validationGroup, bool renderScript = false)
		{

#if TELERIKWEBUI
			var radCaptcha = new Telerik.Web.UI.RadCaptcha
			{
				ID = string.IsNullOrWhiteSpace(controlID) ? DefaultControlID : controlID,
				ValidationGroup = validationGroup,
				ErrorMessage = ErrorMessage,
				Display = ValidatorDisplay.None,
				EnableRefreshImage = true,
				OnClientLoad = "radcaptcha.onClientLoad",
				CaptchaTextBoxLabel = ResourceManager.GetString("CaptchaTextBoxLabel"),
				CaptchaAudioLinkButtonText = ResourceManager.GetString("CaptchaAudioLinkButtonText"),
				CaptchaLinkButtonText = ResourceManager.GetString("CaptchaLinkButtonText"),
				EnableDownloadAudio = false
			};

			radCaptcha.CaptchaImage.EnableCaptchaAudio = true;
			radCaptcha.CaptchaImage.AudioFilesPath = AudioFilesPath;

			container.Controls.Add(radCaptcha);

			if (container.Page != null && renderScript)
			{
				RenderRadCaptchaScript(container.Page);
			}


			return radCaptcha;

#else

			ADXTrace.Instance.TraceWarning(TraceCategory.Application, "Captcha is enabled; however, Telerik.Web.UI.dll could not be found.");
			return null;

#endif
		}

#if TELERIKWEBUI

		/// <summary>
		/// register rad captcha script
		/// </summary>
		/// <param name="page">Currect page where java script will be rendered</param>
		public static void RenderRadCaptchaScript(Page page)
		{
			if (page == null)
			{
				return;
			}
			var scriptManager = ScriptManager.GetCurrent(page);
			if (scriptManager == null)
			{
				return;
			}
			var absolutePath = VirtualPathUtility.ToAbsolute(RadCaptchaScript);
			scriptManager.Scripts.Add(new ScriptReference(absolutePath));

		}

#endif

	}
}
