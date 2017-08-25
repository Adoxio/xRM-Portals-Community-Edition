/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Web;
	using System.Web.Routing;

	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Web;

	/// <summary> The content map data adapter. </summary>
	public abstract class ContentMapDataAdapter
	{
		/// <summary> The content map provider. </summary>
		protected IContentMapProvider ContentMapProvider { get; set; }

		/// <summary> The language. </summary>
		protected ContextLanguageInfo Language { get; set; }

		/// <summary> Gets the dependencies. </summary>
		protected IDataAdapterDependencies Dependencies { get; set; }

		/// <summary> Initializes a new instance of the <see cref="ContentMapDataAdapter"/> class. </summary>
		/// <param name="dependencies"> The dependencies. </param>
		protected ContentMapDataAdapter(IDataAdapterDependencies dependencies)
			: this(dependencies.GetRequestContext(), dependencies)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMapDataAdapter"/> class.
		/// </summary>
		/// <param name="context"> The context. </param>
		/// <param name="dependencies"> The dependencies. </param>
		protected ContentMapDataAdapter(RequestContext context, IDataAdapterDependencies dependencies)
			: this(context != null ? context.HttpContext : new HttpContextWrapper(HttpContext.Current), dependencies)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMapDataAdapter"/> class.
		/// </summary> <param name="context"> The context. </param>
		/// <param name="dependencies"> The dependencies. </param>
		protected ContentMapDataAdapter(HttpContextBase context, IDataAdapterDependencies dependencies)
			: this(context.GetContentMapProvider(), context.GetContextLanguageInfo(), dependencies)
		{
		}

		/// <summary> Initializes a new instance of the <see cref="ContentMapDataAdapter"/> class. </summary>
		/// <param name="contentMapProvider"> The content map provider. </param>
		/// <param name="language"> The language. </param>
		/// <param name="dependencies"> The dependencies. </param>
		protected ContentMapDataAdapter(IContentMapProvider contentMapProvider, ContextLanguageInfo language, IDataAdapterDependencies dependencies)
		{
			if (contentMapProvider == null)
			{
				throw new ArgumentNullException("contentMapProvider");
			}
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			this.ContentMapProvider = contentMapProvider;
			this.Dependencies = dependencies;
			this.Language = language;
		}
	}
}
