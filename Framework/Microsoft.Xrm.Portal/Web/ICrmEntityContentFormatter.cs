/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Provides an interface through which entity content can be transformed before it is rendered.
	/// </summary>
	public interface ICrmEntityContentFormatter
	{
		/// <summary>
		/// Formats/transforms content for rendering.
		/// </summary>
		/// <param name="content">The content to be formatted. This value may be null, and implementations should handle this as a valid case.</param>
		/// <param name="entity">The <see cref="Entity"/> to which the content being formatted belongs.</param>
		/// <param name="context">The context from which this method is called (usually the caller). This value may be null, and implementations should handle this as a valid case.</param>
		/// <returns>The transformed content.</returns>
		string Format(string content, Entity entity, object context);
	}

	internal class PassthroughCrmEntityContentFormatter : ICrmEntityContentFormatter
	{
		public string Format(string content, Entity entity, object context)
		{
			return content;
		}
	}
}
