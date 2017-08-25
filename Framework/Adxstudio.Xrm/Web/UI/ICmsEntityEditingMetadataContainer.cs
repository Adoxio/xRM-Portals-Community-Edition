/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Interface through which <see cref="ICmsEntityEditingMetadataProvider"/> performs metadata
	/// rendering operations.
	/// </summary>
	public interface ICmsEntityEditingMetadataContainer
	{
		void AddAttribute(string name, string value);

		void AddCssClass(string cssClass);

		void AddLabel(string label);

		void AddPicklistMetadata(string entityLogicalName, string attributeLogicalName, Dictionary<int, string> options);

		void AddPreviewPermittedMetadata();

		void AddServiceReference(string servicePath, string cssClass, string title = null);

		void AddSiteMarkerMetadata(string entityLogicalName, string siteMarkerName);

		void AddTagMetadata(string entityLogicalName, IEnumerable<string> tags);
	}
}
