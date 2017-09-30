/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Link for a download action
	/// </summary>
	public class DownloadActionLink : ViewActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-download' aria-hidden='true'></span> " + ResourceManager.GetString("Download_Button_Label");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Download_Button_Label");
		private static string DefaultCurrentPageLabel = ResourceManager.GetString("Download_Records_From_Current_Page_Label");
		private static string DefaultAllPagesLabel = ResourceManager.GetString("Download_Records_From_All_Pages");

		/// <summary>
		/// Format of download
		/// </summary>
		public enum FormatType
		{
			/// <summary>
			/// Comma Delimited (CSV)
			/// </summary>
			Csv = 1,
			/// <summary>
			/// Excel
			/// </summary>
			Excel = 2
		}

		/// <summary>
		/// Label for download the current page data
		/// </summary>
		public string CurrentPageLabel { get; set; }

		/// <summary>
		/// Label for download the all pages data
		/// </summary>
		public string AllPagesLabel { get; set; }

		/// <summary>
		/// The format of the download.
		/// </summary>
		public FormatType Format { get; set; }

		/// <summary>
		/// Parameterless Constructor
		/// </summary>
		public DownloadActionLink()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url">Url to the service to complete the download request.</param>
		/// <param name="currentPageLabel">Label for download of current page data.</param>
		/// <param name="allPagesLabel">Label for download of all pages data.</param>
		/// <param name="label">Text displayed for the button.</param>
		/// <param name="tooltip">Text displayed for a tooltip.</param>
		/// <param name="enabled">Indicates if the link is enabled or not.</param>
		/// <param name="format">Format of the download</param>
		public DownloadActionLink(UrlBuilder url, string label = null, string tooltip = null, string currentPageLabel = null, string allPagesLabel = null, bool enabled = true, FormatType format = FormatType.Excel) : base(LinkActionType.Download, enabled, url, label, tooltip, null)
		{
			label = DefaultButtonLabel == null ? DefaultButtonLabel : label;
			label = DefaultButtonTooltip == null ? DefaultButtonTooltip : label;
			CurrentPageLabel = currentPageLabel == null ? DefaultCurrentPageLabel : currentPageLabel;
			AllPagesLabel = allPagesLabel == null ? DefaultAllPagesLabel : allPagesLabel;
			Format = format;
		}

		public DownloadActionLink(IPortalContext portalContext, int languageCode, DownloadAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.Download, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
			var allPagesLabel = Localization.GetLocalizedString(action.AllPagesLabel, languageCode);
			var currentPageLabel = Localization.GetLocalizedString(action.CurrentPageLabel, languageCode);
			
			AllPagesLabel = !string.IsNullOrWhiteSpace(allPagesLabel) ? allPagesLabel : DefaultAllPagesLabel;
			CurrentPageLabel = !string.IsNullOrWhiteSpace(currentPageLabel) ? currentPageLabel : DefaultCurrentPageLabel;
			Format = FormatType.Excel;
			Type = LinkActionType.Download;

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("DownloadAsExcel", "EntityGrid",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
