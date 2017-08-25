/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Interface definition for WebsiteLanguage.
	/// </summary>
	public interface IWebsiteLanguage
	{
		/// <summary>
		/// Gets a reference to the corresponding WebsiteLanguage entity.
		/// </summary>
		EntityReference EntityReference { get; }

		/// <summary>
		/// Gets a reference to the corresponding WebsiteLanguage EntityNode.
		/// </summary>
		WebsiteLanguageNode WebsiteLanguageNode { get; }

		/// <summary>
		/// Gets this language's name (i.e. the name of the corresponding Portal Language entity).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets this language's localized portal display name (i.e. the Portal Display Name field of the corresponding Portal Language entity).
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Gets the localizable language code (i.e. the Code field of the corresponding Portal Language entity).
		/// Ex: "en-US".
		/// </summary>
		string Code { get; }
		
		/// <summary>
		/// Gets the LCID of this language's corresponding CRM language (i.e. the CRM Language field of the corresponding Portal Language entity).
		/// Ex: 1033.
		/// </summary>
		int CrmLcid { get; }

		/// <summary>
		/// Gets the localizable LCID (i.e. the LCID field of the corresponding Portal Language entity).
		/// Ex: 1033.
		/// </summary>
		int Lcid { get; }

		/// <summary>
		/// Gets whether this website language is published.
		/// </summary>
		bool IsPublished { get; }

		/// <summary>
		/// Gets the ID of the corresponding Portal language entity.
		/// </summary>
		Guid PortalLanguageId { get; }

		/// <summary>
		/// Gets or sets if this language was selected as a fallback from another language lcid or code 
		/// </summary>
		bool UsedAsFallback { get; set; }
	}
}
