/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Represents a SiteMap node for the site map providers.
	/// </summary>
	public class CrmSiteMapNode : SiteMapNode // MSBug #120131: Won't seal, inheritance is expected extension point.
	{
		private readonly HttpStatusCode? _statusCode;

		/// <summary>
		/// Constructs a new site map node.
		/// </summary>
		/// <param name="provider">Provider used to construct this site map node.</param>
		/// <param name="key">Unique key.</param>
		/// <param name="url">Url to render.</param>
		/// <param name="title">Title of page.</param>
		/// <param name="description">Description of page.</param>
		/// <param name="rewriteUrl">Url to use for url rewriting.</param>
		/// <param name="lastModified">Last modification date.</param>
		protected CrmSiteMapNode(
			SiteMapProvider provider,
			string key,
			string url,
			string title,
			string description,
			string rewriteUrl,
			DateTime lastModified) : base(provider, key, url, title, description)
		{
			RewriteUrl = rewriteUrl;
			LastModified = lastModified;
		}

		public CrmSiteMapNode(
			SiteMapProvider provider,
			string key,
			string url,
			string title,
			string description,
			string rewriteUrl,
			DateTime lastModified,
			Entity entity)
			: this(provider, key, url, title, description, rewriteUrl, lastModified)
		{
			entity.ThrowOnNull("entity");

			Entity = entity;
		}

		public CrmSiteMapNode(
			SiteMapProvider provider,
			string key,
			string url,
			string title,
			string description,
			string rewriteUrl,
			DateTime lastModified,
			Entity entity,
			HttpStatusCode statusCode)
			: this(provider, key, url, title, description, rewriteUrl, lastModified, entity)
		{
			_statusCode = statusCode;
		}

		/// <summary>
		/// The real url to use for url rewriting for site map node.
		/// </summary>
		public string RewriteUrl { get; private set; }
		
		/// <summary>
		/// The last modification date of the data for site map node.
		/// </summary>
		public DateTime LastModified { get; private set; }

		public Entity Entity { get; private set; }

		public virtual HttpStatusCode StatusCode
		{
			get { return _statusCode.HasValue ? _statusCode.Value : HttpStatusCode.OK; }
		}

		public bool HasCrmEntityName(string entityName)
		{
			return Entity.LogicalName == entityName;
		}
	}
}
