/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Renders the value (adx_value) property of a content snippet (adx_contentsnippet).
	/// </summary>
	public class Snippet : Property
	{
		private bool? _allowCreate;

		public bool AllowCreate
		{
			get { return _allowCreate.GetValueOrDefault(SnippetExtensions.AllowCreateDefault); }
			set { _allowCreate = value; }
		}

		public string SnippetName { get; set; }

		/// <summary>
		/// Gets/Sets the DisplayName property for the snippet record
		/// </summary>
		public string DisplayName { get; set; }

		protected override string GetEditDisplayName(Entity entity, string propertyName)
		{
			if (DisplayName == null)
			{
				DisplayName = entity.GetAttributeValue<string>("adx_display_name");
			}
					
			return DisplayName ?? SnippetName ?? base.GetEditDisplayName(entity, propertyName);
		}

		protected override void OnLoad(EventArgs args)
		{
			var portalViewContext = new PortalViewContext(
				new PortalContextDataAdapterDependencies(
					PortalCrmConfigurationManager.CreatePortalContext(PortalName),
					PortalName,
					Context.Request.RequestContext));

			var snippet = portalViewContext.Snippets.Select(SnippetName);

			if (snippet == null)
			{
				DataItem = GetDefaultDataItem();
			}
			else
			{
				DataItem = snippet.Entity;

				if (string.IsNullOrEmpty(PropertyName))
				{
					PropertyName = "adx_value";
				}
			}

			base.OnLoad(args);
		}

		private object GetDefaultDataItem()
		{
			if (AllowCreate && !Literal)
			{
				return Html.SnippetPlaceHolder(SnippetName, EditType, HtmlEncode, TagName, CssClass, LiquidEnabled, defaultValue: DefaultText, displayName: DisplayName);
			}

			if (DefaultText == null)
			{
				return null;
			}

			if (LiquidEnabled)
			{
				return new HtmlString(Html.Liquid(DefaultText));
			}

			return new HtmlString(DefaultText);
		}
	}
}
