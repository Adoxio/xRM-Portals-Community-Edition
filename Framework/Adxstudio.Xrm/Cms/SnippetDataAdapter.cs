/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.Xrm.Client;

	/// <summary> The snippet data adapter. </summary>
	public class SnippetDataAdapter : ContentMapDataAdapter, ISnippetDataAdapter
	{
		/// <summary> Initializes a new instance of the <see cref="SnippetDataAdapter"/> class. </summary>
		/// <param name="dependencies"> The dependencies. </param>
		public SnippetDataAdapter(IDataAdapterDependencies dependencies)
			: base(dependencies)
		{
		}

		/// <summary> Selects the Snippet by name. </summary>
		/// <param name="snippetName"> The snippet name. </param>
		/// <returns> The <see cref="ISnippet"/>. </returns>
		public ISnippet Select(string snippetName)
		{
			if (string.IsNullOrEmpty(snippetName))
			{
				return null;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", snippetName));

			var snippetNode = this.ContentMapProvider.Using(contentMap => contentMap.GetSnippetNode(snippetName, this.Language));

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", snippetName));

			return this.ToSnippet(snippetNode);
		}

		/// <summary> The to snippet. </summary>
		/// <param name="snippetNode"> The snippet node. </param>
		/// <returns> The <see cref="ISnippet"/>. </returns>
		private ISnippet ToSnippet(ContentSnippetNode snippetNode)
		{
			if (snippetNode == null)
			{
				return null;
			}

			var urlProvider = this.Dependencies.GetUrlProvider();
			var securityProvider = this.Dependencies.GetSecurityProvider();
			var serviceContext = this.Dependencies.GetServiceContext();
			var entity = serviceContext.MergeClone(snippetNode.ToEntity());

			var portalViewEntity = new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider);
			var snippet = new Snippet(entity, portalViewEntity, this.Language);

			return snippet;
		}
	}
}
