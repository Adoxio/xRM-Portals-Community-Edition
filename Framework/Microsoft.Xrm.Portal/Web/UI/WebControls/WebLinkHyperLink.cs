/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Renders a hyperlink for a given adx_weblink entity.
	/// </summary>
	public class WebLinkHyperLink : HyperLink // MSBug #120118: Won't seal, inheritance is expected extension point.
	{
		private bool _showImage = true;

		public string PortalName { get; set; }

		/// <summary>
		/// Gets or sets a CSS class value to be added to the hyperlink if the target node of
		/// the hyperlink is the current site map node.
		/// </summary>
		[Description("A CSS class value to be added to the weblink if the target node of the weblink is the current site map node")]
		[Category("Data")]
		[DefaultValue((string)null)]
		public string CurrentSiteMapNodeCssClass { get; set; }

		/// <summary>
		/// Gets or sets a CSS class value to be added to the hyperlink if the current site map
		/// node is a descendant of the target node of the hyperlink.
		/// </summary>
		[Description("A CSS class value to be added to the weblink if the current site map node is a descendant of the target node of the weblink")]
		[Category("Data")]
		[DefaultValue((string)null)]
		public string ParentOfCurrentSiteMapNodeCssClass { get; set; }

		/// <summary>
		/// Gets or sets a Boolean indicating whether or not this hyperlink should render an image
		/// link, if an image is associated with the given adx_weblink.
		/// </summary>
		public bool ShowImage
		{
			get { return _showImage; }
			set { _showImage = value; }
		}

		/// <summary>
		/// Gets or sets the adx_weblink entity for which a corresponding hyperlink will be rendered.
		/// </summary>
		public Entity WebLink { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			if (WebLink != null)
			{
				WebLink.AssertEntityName("adx_weblink");

				var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
				var context = portal.ServiceContext;

				var weblinkUrl = context.GetUrl(context.MergeClone(WebLink));

				NavigateUrl = weblinkUrl;

				if (weblinkUrl != null && string.IsNullOrEmpty(WebLink.GetAttributeValue<string>("adx_externalurl")))
				{
					var cssClasses = new List<string>();
					
					if (!string.IsNullOrEmpty(CurrentSiteMapNodeCssClass) && IsCurrentNode(weblinkUrl))
					{
					    cssClasses.Add(CurrentSiteMapNodeCssClass);
					}

					if (!string.IsNullOrEmpty(ParentOfCurrentSiteMapNodeCssClass) && IsParentOfCurrentNode(weblinkUrl))
					{
					    cssClasses.Add(ParentOfCurrentSiteMapNodeCssClass);
					}

					if (cssClasses.Any())
					{
						CssClass = "{0} {1}".FormatWith(CssClass, string.Join(" ", cssClasses.ToArray()));
					}
				}

				if (string.IsNullOrEmpty(Target) && WebLink.GetAttributeValue<bool?>("adx_openinnewwindow").GetValueOrDefault(false))
				{
					Target = "_blank";
				}

				if (!WebLink.GetAttributeValue<bool?>("adx_robotsfollowlink").GetValueOrDefault(true))
				{
					Attributes["rel"] = "nofollow";
				}

				var contentFormatter = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityContentFormatter>(GetType().FullName) ?? new PassthroughCrmEntityContentFormatter();

				var name = contentFormatter.Format(WebLink.GetAttributeValue<string>("adx_name"), WebLink, this);

				if (string.IsNullOrEmpty(ToolTip))
				{
					ToolTip = name;
				}

				var imageUrl = WebLink.GetAttributeValue<string>("adx_imageurl");

				if (ShowImage && !string.IsNullOrEmpty(imageUrl))
				{
					var image = new Image
					{
						ImageUrl = imageUrl.StartsWith("~/") ? VirtualPathUtility.ToAppRelative(imageUrl) : imageUrl,
						AlternateText = contentFormatter.Format(WebLink.GetAttributeValue<string>("adx_imagealttext"), WebLink, this)
					};

					var weblinkImageHeight = WebLink.GetAttributeValue<int?>("adx_imageheight");

					if (weblinkImageHeight.HasValue)
					{
						image.Height = weblinkImageHeight.Value;
					}

					var weblinkImageWidth = WebLink.GetAttributeValue<int?>("adx_imagewidth");

					if (weblinkImageWidth.HasValue)
					{
						image.Width = weblinkImageWidth.Value;
					}

					Controls.Add(image);
				}
				else if (string.IsNullOrEmpty(Text))
				{
					Text = name;
				}
			}

			base.OnPreRender(e);
		}

		private static bool IsCurrentNode(string url)
		{
			if (!SiteMap.Enabled || SiteMap.CurrentNode == null)
			{
				return false;
			}

			var node = SiteMap.Provider.FindSiteMapNode(url);

			return node != null && SiteMap.CurrentNode.Key == node.Key && node.Url == url;
		}

		private static bool IsParentOfCurrentNode(string url)
		{
			if (!SiteMap.Enabled || SiteMap.CurrentNode == null)
			{
				return false;
			}

			var node = SiteMap.Provider.FindSiteMapNode(url);

			return node != null && SiteMap.CurrentNode.IsDescendantOf(node);
		}
	}
}
