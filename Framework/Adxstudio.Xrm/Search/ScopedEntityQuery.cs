/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Search
{
	public class ScopedEntityQuery : CrmEntityQuery, IScopedEntityQuery
	{
		public ScopedEntityQuery(IEnumerable<string> scopes, string queryText, int pageNumber, int pageSize, IEnumerable<string> logicalNames, IWebsiteLanguage language, bool multiLanguageEnabled, string filter = null) 
			: base(queryText, pageNumber, pageSize, logicalNames, language, multiLanguageEnabled, filter)
		{
			if (scopes == null)
			{
				throw new ArgumentNullException("scopes");
			}

			Scopes = scopes.ToArray();
		}

		public ScopedEntityQuery(IEnumerable<string> scopes, string queryText, int pageNumber, int pageSize, IWebsiteLanguage language, bool multiLanguageEnabled, string filter = null) 
			: base(queryText, pageNumber, pageSize, language, multiLanguageEnabled, filter)
		{
			if (scopes == null)
			{
				throw new ArgumentNullException("scopes");
			}

			Scopes = scopes.ToArray();
		}

		public IEnumerable<string> Scopes { get; private set; }
	}
}
