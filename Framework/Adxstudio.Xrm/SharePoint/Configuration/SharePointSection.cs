/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;

namespace Adxstudio.SharePoint.Configuration
{
	/// <summary>
	/// Represents a collection of site configuration nodes and settings for all sites in general.
	/// </summary>
	internal sealed class SharePointSection : ConfigurationSection
	{
		/// <summary>
		/// The element name of the section.
		/// </summary>
		public const string SectionName = "adxstudio.xrm.sharePoint";

		private static readonly ConfigurationPropertyCollection _properties;

		static SharePointSection()
		{
			_properties = new ConfigurationPropertyCollection { };
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		protected override void Reset(ConfigurationElement parentElement)
		{
			base.Reset(parentElement);
			SharePointConfigurationManager.Reset();
		}
	}
}
