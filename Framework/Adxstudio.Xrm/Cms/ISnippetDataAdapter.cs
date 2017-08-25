/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface ISnippetDataAdapter
	{
		ISnippet Select(string snippetName);
	}

	internal class RequestCachingSnippetDataAdapter : RequestCachingDataAdapter, ISnippetDataAdapter
	{
		private readonly ISnippetDataAdapter _snippets;

		public RequestCachingSnippetDataAdapter(ISnippetDataAdapter snippets, EntityReference website) : base("{0}:{1}".FormatWith(snippets.GetType().FullName, website.Id))
		{
			if (snippets == null) throw new ArgumentNullException("snippets");
			if (website == null) throw new ArgumentNullException("website");

			_snippets = snippets;
		}

		public ISnippet Select(string snippetName)
		{
			return Get("Select:" + snippetName, () => _snippets.Select(snippetName));
		}
	}
}
