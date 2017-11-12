/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Resources;
using System.Web;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	internal class WebFormFunctions
	{
		internal static string DefaultAttachFileLabel = string.Empty;
		internal static string DefaultPreviousButtonCssClass = "button previous";
		internal static string DefaultNextButtonCssClass = "button next";
		internal static string DefaultSubmitButtonCssClass = "button submit";
		public static readonly string DefaultPreviousButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Previous_Button_Label"));
		public static readonly string DefaultNextButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Next_Button_Text"));
		public static readonly string DefaultSubmitButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Submit_Button_Label_Text"));
		public static readonly string DefaultSubmitButtonBusyText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Default_Modal_Processing_Text"));
	}
}
