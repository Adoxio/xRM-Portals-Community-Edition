/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Renders a web link set (adx_weblinkset) as a list of links.
	/// </summary>
	public class WebLinks : EditableCrmEntityDataBoundControl // MSBug #120119: Won't seal, Inheritance is expected extension point.
	{
		private bool _showCopy = true;
		private bool _showLinkDescriptions = true;
		private bool _showImage = true;
		private bool _showTitle = true;

		/// <summary>
		/// Gets or sets a CSS class value to be added to the hyperlink if the target node of
		/// the hyperlink is the current site map node.
		/// </summary>
		[Description("A CSS class value to be added to a weblink if the target node of the weblink is the current site map node")]
		[Category("Data")]
		[DefaultValue((string)null)]
		public string CurrentSiteMapNodeCssClass { get; set; }

		/// <summary>
		/// Gets or sets a CSS class value to be added to the hyperlink if the current site map
		/// node is a descendant of the target node of the hyperlink.
		/// </summary>
		[Description("A CSS class value to be added to a weblink if the current site map node is a descendant of the target node of the weblink")]
		[Category("Data")]
		[DefaultValue((string)null)]
		public string ParentOfCurrentSiteMapNodeCssClass { get; set; }

		public string DescriptionCssClass { get; set; }

		public bool ShowCopy
		{
			get { return _showCopy; }
			set { _showCopy = value; }
		}

		public bool ShowImage
		{
			get { return _showImage; }
			set { _showImage = value; }
		}

		public bool ShowLinkDescriptions
		{
			get { return _showLinkDescriptions; }
			set { _showLinkDescriptions = value; }
		}

		public bool ShowTitle
		{
			get { return _showTitle; }
			set { _showTitle = value; }
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		public string WebLinkSetName { get; set; }

		protected override void OnLoad(EventArgs args)
		{
			Entity webLinkSet;

			if (TryGetWebLinkSetEntity(WebLinkSetName, out webLinkSet))
			{
				DataItem = webLinkSet;
			}

			base.OnLoad(args);
		}

		protected override void PerformDataBindingOfCrmEntity(Entity entity)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var context = portal.ServiceContext;

			entity = context.MergeClone(entity);

			if (ShowTitle)
			{
				Controls.Add(new Property
				{
					PropertyName = GetPropertyName(context, entity, "adx_title"),
					CssClass = "weblinkset-title",
					EditType = "text",
					DataItem = entity
				});
			}

			if (ShowCopy)
			{
				Controls.Add(new Property
				{
					PropertyName = GetPropertyName(context, entity, "adx_copy"),
					CssClass = "weblinkset-copy",
					EditType = "html",
					DataItem = entity
				});
			}

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			var weblinks = entity.GetRelatedEntities(context, "adx_weblinkset_weblink")
				.Where(e => securityProvider.TryAssert(context, e, CrmEntityRight.Read))
				.OrderBy(weblink => weblink.GetAttributeValue<int?>("adx_displayorder"))
				.ToList();

			var weblinkCount = weblinks.Count();

			var listItems = weblinks.Select((weblink, index) =>
			{
				var li = new HtmlGenericControl("li");

				SetPositionalClassAttribute(li, weblinkCount, index);

				if (ItemTemplate != null)
				{
					var item = CreateItem(this, index, ListItemType.Item, true, weblink);

					Controls.Remove(item);
					li.Controls.Add(item);
				}
				else
				{
					li.Controls.Add(GetHyperLinkForWebLink(weblink));

					if (ShowLinkDescriptions)
					{
						var description = new HtmlGenericControl("div");

						description.Controls.Add(new Property
						{
							PropertyName = GetPropertyName(context, weblink, "adx_description"),
							DataItem = weblink,
							Literal = true
						});

						if (!string.IsNullOrEmpty(DescriptionCssClass))
						{
							description.Attributes["class"] = DescriptionCssClass;
						}

						li.Controls.Add(description);
					}
				}

				return li;
			});

			var container = new HtmlGenericControl("div");
			var containerCssClasses = new List<string> { "weblinkset-weblinks" };

			Controls.Add(container);

			if (listItems.Any())
			{
				var list = new HtmlGenericControl("ul");

				foreach (var li in listItems)
				{
					list.Controls.Add(li);
				}

				container.Controls.Add(list);
			}

			if (Editable)
			{
				containerCssClasses.Add("xrm-entity");
				containerCssClasses.Add("xrm-editable-{0}".FormatWith(entity.LogicalName));

				if (HasEditPermission(entity))
				{
					var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityEditingMetadataProvider>();

					metadataProvider.AddEntityMetadata(PortalName, this, container, entity);

					this.RegisterClientSideDependencies();
				}
			}

			container.Attributes["class"] = string.Join(" ", containerCssClasses.ToArray());
		}

		protected override void PerformDataBindingOfCrmEntityProperty(Entity entity, string propertyName, string value)
		{
			PerformDataBindingOfCrmEntity(entity);
		}

		private HyperLink GetHyperLinkForWebLink(Entity weblink)
		{
			var hyperLink = new WebLinkHyperLink
			{
				WebLink = weblink, ShowImage = ShowImage, CurrentSiteMapNodeCssClass = CurrentSiteMapNodeCssClass, ParentOfCurrentSiteMapNodeCssClass = ParentOfCurrentSiteMapNodeCssClass, PortalName = PortalName
			};

			if (Editable)
			{
				hyperLink.CssClass = "xrm-weblink {0}".FormatWith(hyperLink.CssClass);
			}

			return hyperLink;
		}

		private bool TryGetWebLinkSetEntity(string webLinkSetName, out Entity webLinkSet)
		{
			webLinkSet = null;

			if (string.IsNullOrEmpty(webLinkSetName))
			{
				return false;
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var context = portal.ServiceContext;

			webLinkSet = context.GetLinkSetByName(portal.Website, WebLinkSetName);

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			return webLinkSet != null && securityProvider.TryAssert(context, webLinkSet, CrmEntityRight.Read);
		}

		private static void SetPositionalClassAttribute(HtmlControl control, int weblinkCount, int index)
		{
			var positionalCssClasses = new List<string>();

			if (index == 0)
			{
				positionalCssClasses.Add("first");
			}

			if (index == (weblinkCount - 1))
			{
				positionalCssClasses.Add("last");
			}

			if (weblinkCount == 1)
			{
				positionalCssClasses.Add("only");
			}

			if (positionalCssClasses.Count > 0)
			{
				control.Attributes["class"] = string.Join(" ", positionalCssClasses.ToArray());
			}
		}

		private static string GetPropertyName(OrganizationServiceContext context, Entity entity, string logicalName)
		{
			EntitySetInfo esi;
			AttributeInfo ai;
			
			if (OrganizationServiceContextInfo.TryGet(context, entity, out esi)
				&& esi.Entity.AttributesByLogicalName.TryGetValue(logicalName, out ai))
			{
				return ai.Property.Name;
			}

			throw new InvalidOperationException("The '{0}' entity does not contain an attribute with the logical name '{1}'.".FormatWith(entity, logicalName));
		}
	}
}
