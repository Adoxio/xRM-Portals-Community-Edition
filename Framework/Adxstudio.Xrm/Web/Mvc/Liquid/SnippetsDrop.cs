/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc.Html;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SnippetsDrop : PortalDrop, IEditableCollection
	{
		private readonly ISnippetDataAdapter _snippets;

		public SnippetsDrop(IPortalLiquidContext portalLiquidContext, ISnippetDataAdapter snippets) : base(portalLiquidContext)
		{
			if (snippets == null) throw new ArgumentNullException("snippets");

			_snippets = snippets;
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			var snippet = _snippets.Select(method);

			return snippet == null || snippet.Value == null ? null : snippet.Value.Value;
		}

		public virtual string GetEditable(Context context, string key, EditableOptions options)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (key == null) throw new ArgumentNullException("key");
			if (options == null) throw new ArgumentNullException("options");

			IHtmlString html = null;

			context.Stack(() =>
			{
				html = Html.SnippetInternal(
					key,
					options.Type ?? "html",
					options.Escape.GetValueOrDefault(false),
					options.Tag ?? "div",
					options.CssClass,
					options.Liquid.GetValueOrDefault(true),
					context,
					options.Default);
			});

			return html == null ? null : html.ToString();
		}
	}
}
