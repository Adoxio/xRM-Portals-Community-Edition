/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.Web;
using Microsoft.Xrm.Portal.Web.Handlers;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// A group of <see cref="HttpCachePolicyElement"/> elements used globally by the portal.
	/// </summary>
	public sealed class PortalCachePolicyElement : ConfigurationElement
	{
		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propEmbeddedResource;
		private static readonly ConfigurationProperty _propAnnotation;

		static PortalCachePolicyElement()
		{
			_propEmbeddedResource = new ConfigurationProperty("embeddedResource", typeof(HttpCachePolicyElement), new HttpCachePolicyElement(), ConfigurationPropertyOptions.None);
			_propAnnotation = new ConfigurationProperty("annotation", typeof(HttpCachePolicyElement), new HttpCachePolicyElement(), ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propEmbeddedResource, _propAnnotation };
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		/// <summary>
		/// The settings in which the <see cref="HttpCachePolicy"/> of the <see cref="HttpResponse"/> will cache rendered embedded resources.
		/// </summary>
		/// <remarks>
		/// Used by the <see cref="EmbeddedResourceHttpHandler"/> for setting response headers.
		/// </remarks>
		[ConfigurationProperty("embeddedResource")]
		public HttpCachePolicyElement EmbeddedResource
		{
			get { return (HttpCachePolicyElement)base[_propEmbeddedResource]; }
			set { base[_propEmbeddedResource] = value; }
		}

		/// <summary>
		/// The settings in which the <see cref="HttpCachePolicy"/> of the <see cref="HttpResponse"/> will cache rendered annotations.
		/// </summary>
		/// <remarks>
		/// Used by the <see cref="AnnotationHandler"/> for setting response headers.
		/// </remarks>
		[ConfigurationProperty("annotation")]
		public HttpCachePolicyElement Annotation
		{
			get { return (HttpCachePolicyElement)base[_propAnnotation]; }
			set { base[_propAnnotation] = value; }
		}
	}
}
