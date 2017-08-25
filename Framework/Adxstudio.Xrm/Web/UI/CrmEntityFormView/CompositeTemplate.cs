/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	internal sealed class CompositeTemplate : ITemplate
	{
		private readonly IEnumerable<ITemplate> _templates;

		public CompositeTemplate(IEnumerable<ITemplate> templates)
		{
			templates.ThrowOnNull("templates");

			_templates = templates;
		}

		public void InstantiateIn(Control container)
		{
			foreach (var template in _templates)
			{
				template.InstantiateIn(container);
			}
		}
	}
}
