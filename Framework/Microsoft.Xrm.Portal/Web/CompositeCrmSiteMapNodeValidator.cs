/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Composes multiple <see cref="ICrmSiteMapNodeValidator"/>s into a single validator for which
	/// the <see cref="Validate"/> method on each composed validator must return true.
	/// </summary>
	public sealed class CompositeCrmSiteMapNodeValidator : ICrmSiteMapNodeValidator, IEnumerable<ICrmSiteMapNodeValidator>
	{
		private readonly List<ICrmSiteMapNodeValidator> _validators = new List<ICrmSiteMapNodeValidator>();

		public CompositeCrmSiteMapNodeValidator() { }

		public CompositeCrmSiteMapNodeValidator(IEnumerable<ICrmSiteMapNodeValidator> validators)
		{
			_validators.AddRange(validators);
		}

		public void Add(ICrmSiteMapNodeValidator validator)
		{
			_validators.Add(validator);
		}

		public bool Validate(OrganizationServiceContext context, CrmSiteMapNode node)
		{
			return _validators.All(validator => validator.Validate(context, node));
		}

		public IEnumerator<ICrmSiteMapNodeValidator> GetEnumerator()
		{
			return _validators.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
