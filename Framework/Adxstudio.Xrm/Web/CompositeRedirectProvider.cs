/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web
{
	public class CompositeRedirectProvider : IRedirectProvider
	{
		private readonly IEnumerable<IRedirectProvider> _providers;

		public CompositeRedirectProvider(IEnumerable<IRedirectProvider> providers)
		{
			if (providers == null)
			{
				throw new ArgumentNullException("providers");
			}

			_providers = providers;
		}

		public CompositeRedirectProvider(params IRedirectProvider[] providers) : this((IEnumerable<IRedirectProvider>)providers) { }

		public IRedirectMatch Match(Guid websiteID, UrlBuilder url)
		{
			foreach (var provider in _providers)
			{
				var match = provider.Match(websiteID, url);

				if (match.Success)
				{
					return match;
				}
			}

			return new FailedRedirectMatch();
		}
	}
}
