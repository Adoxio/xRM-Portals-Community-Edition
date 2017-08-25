/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// The set of options for auto-generated container elements.
	/// </summary>
	public enum CrmListViewListMode
	{
		None,
		UnorderedList,
		OrderedList,
		Div,
		Table
	}

	/// <summary>
	/// The base <see cref="T:System.Web.UI.WebControls.ListView"></see> control for rendering XRM entity content.
	/// </summary>
	public class CrmListView : ListView
	{
		/// <summary>
		/// Gets or sets the mode for controlling auto-generated container elements.
		/// </summary>
		public CrmListViewListMode ListMode { get; set; }

		/// <summary>
		/// Gets or sets the CSS class to be applied to the container list element if auto-generated containers is enabled.
		/// </summary>
		[CssClassProperty, Category("Appearance"), DefaultValue("")]
		public string ListCssClass { get; set; }

		/// <summary>
		/// Gets or sets the CSS class to be applied to the list item elements if auto-generated containers is enabled.
		/// </summary>
		[CssClassProperty, Category("Appearance"), DefaultValue("")]
		public string ItemCssClass { get; set; }

		/// <summary>
		/// Gets or sets the element name used to render the container element. Overrides the name selected by the ListMode setting.
		/// </summary>
		public string ListTagName { get; set; }

		/// <summary>
		/// Gets or sets the element name used to render the item elements. Overrides the name selected by the ListMode setting.
		/// </summary>
		public string ItemTagName { get; set; }

		protected virtual void RegisterDefaultTemplates()
		{
			LayoutTemplate = CreateTemplate(LayoutTemplate, CreateLayoutTemplate);
			ItemTemplate = CreateTemplate(ItemTemplate, CreateItemTemplate);

			if (ListMode != CrmListViewListMode.None)
			{
				GroupTemplate = CreateTemplate(GroupTemplate, CreateGroupTemplate);
			}
		}

		protected override void CreateLayoutTemplate()
		{
			RegisterDefaultTemplates();

			base.CreateLayoutTemplate();
		}

		protected virtual Control CreateItemTemplate(ListViewDataItem item)
		{
			return null;
		}

		protected virtual Control CreateLayoutTemplate(ListViewDataItem item)
		{
			switch (ListMode)
			{
				case CrmListViewListMode.UnorderedList:
					return CreateListContainer("ul", item);
				case CrmListViewListMode.OrderedList:
					return CreateListContainer("ol", item);
				case CrmListViewListMode.Div:
					return CreateListContainer("div", item);
				case CrmListViewListMode.Table:
					return CreateListContainer("table", item);
			}

			return CreateItemPlaceHolder();
		}

		protected virtual Control CreateGroupTemplate(ListViewDataItem item)
		{
			switch (ListMode)
			{
				case CrmListViewListMode.UnorderedList:
				case CrmListViewListMode.OrderedList:
					return CreateListItemContainer("li", item);
				case CrmListViewListMode.Div:
					return CreateListItemContainer("div", item);
				case CrmListViewListMode.Table:
					return CreateListItemContainer("tr", item);
			}

			return CreateItemPlaceHolder();
		}

		protected static ITemplate CreateTemplate(ITemplate template, Func<ListViewDataItem, Control> build)
		{
			// return the explicitly declared template if it exists, otherwise generate a template
			return CreateTemplate(template, null, build);
		}

		protected static ITemplate CreateTemplate(ITemplate template, ITemplate fallback, Func<ListViewDataItem, Control> build)
		{
			return template ?? fallback ?? CreateTemplate(build);
		}

		private static void CreateTemplate(Control container, Func<ListViewDataItem, Control> build)
		{
			// create a template instance control given a container
			var item = container as ListViewDataItem;
			var control = build(item);

			// append the template control to the container
			if (control != null)
			{
				container.Controls.Add(control);
			}
		}

		private static ITemplate CreateTemplate(Func<ListViewDataItem, Control> build)
		{
			// if the call to create template results in a null, then do not return a compiled template
			return new CompiledTemplateBuilder(container => CreateTemplate(container, build));
		}

		private static PlaceHolder CreateItemPlaceHolder()
		{
			return new PlaceHolder { ID = "itemPlaceHolder" };
		}

		private static PlaceHolder CreateGroupPlaceHolder()
		{
			return new PlaceHolder { ID = "groupPlaceHolder" };
		}

		private Control CreateListContainer(string tagName, ListViewDataItem item)
		{
			var container = new HtmlGenericControl(ListTagName ?? tagName);
			container.ID = ID;
			container.Controls.Add(CreateGroupPlaceHolder());

			if (!string.IsNullOrEmpty(ListCssClass))
			{
				container.Attributes["class"] = ListCssClass;
			}

			return container;
		}

		private Control CreateListItemContainer(string tagName, ListViewDataItem item)
		{
			var container = new HtmlGenericControl(ItemTagName ?? tagName);
			container.ID = item != null ? item.ID : null;
			container.Controls.Add(CreateItemPlaceHolder());

			if (!string.IsNullOrEmpty(ItemCssClass))
			{
				container.Attributes["class"] = ItemCssClass;
			}

			return container;
		}
	}
}
