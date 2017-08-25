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
	/// Class encapsulating the information in a WebsiteLanguage entity.
	/// </summary>
	internal class WebsiteLanguage : IWebsiteLanguage
	{
		/// <summary>
		/// Gets this language's name (i.e. the name of the corresponding Portal Language entity).
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets this language's localized portal display name (i.e. the Portal Display Name field of the corresponding Portal Language entity).
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		/// Gets the localizable language code (i.e. the Code field of the corresponding Portal Language entity).
		/// Ex: "en-US".
		/// </summary>
		public string Code { get; private set; }

		/// <summary>
		/// Gets the LCID of this language's corresponding CRM language (i.e. the CRM Language field of the corresponding Portal Language entity).
		/// Ex: 1033.
		/// </summary>
		public int CrmLcid { get; private set; }

		/// <summary>
		/// Gets the localizable LCID (i.e. the LCID field of the corresponding Portal Language entity).
		/// Ex: 1033.
		/// </summary>
		public int Lcid { get; private set; }

		/// <summary>
		/// Gets the ID of the corresponding Portal language entity.
		/// </summary>
		public Guid PortalLanguageId { get; private set; }

		/// <summary>
		/// Gets or sets if this language was selected as a fallback from another language lcid or code 
		/// </summary>
		public bool UsedAsFallback { get; set; }

		/// <summary>
		/// Gets whether this website language is published.
		/// </summary>
		public bool IsPublished { get; private set; }

		/// <summary>
		/// Gets a reference to the corresponding WebsiteLanguage entity.
		/// </summary>
		public EntityReference EntityReference { get; private set; }

		/// <summary>
		/// Gets a reference to the corresponding WebsiteLanguage EntityNode.
		/// </summary>
		public WebsiteLanguageNode WebsiteLanguageNode { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteLanguage" /> class.
		/// </summary>
		/// <param name="name">Name of the corresponding CRM language (i.e. the name of the corresponding Portal Language entity).</param>
		/// <param name="displayName">Localized portal display name (i.e. the Portal Display Name field of the corresponding Portal Language entity).</param>
		/// <param name="code">Localizable language code (i.e. the Code field of the corresponding Portal Language entity).</param>
		/// <param name="crmLcid">LCID of this language's corresponding CRM language (i.e. the CRM Language field of the corresponding Portal Language entity).</param>
		/// <param name="lcid">Localizable LCID (i.e. the LCID field of the corresponding Portal Language entity).</param>
		/// <param name="portalLanguageId">ID of the corresponding Portal language entity.</param>
		/// <param name="isPublished">Whether the language is published.</param>
		/// <param name="websiteLanguageNode">Reference to the corresponding WebsiteLanguage EntityNode.</param>
		public WebsiteLanguage(string name, string displayName, string code, int crmLcid, int lcid, Guid portalLanguageId, bool isPublished, WebsiteLanguageNode websiteLanguageNode)
		{
			this.Name = name;
			this.DisplayName = displayName;
			this.Code = code;
			this.CrmLcid = crmLcid;
			this.Lcid = lcid;
			this.PortalLanguageId = portalLanguageId;
			this.IsPublished = isPublished;
			this.EntityReference = websiteLanguageNode != null ? websiteLanguageNode.ToEntityReference() : null;
			this.WebsiteLanguageNode = websiteLanguageNode;
		}
	}
}
