/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Override of <see cref="DataPager"/> that renders a ul instead of a span and supports a CSS class on that ul.
	/// </summary>
	public class UnorderedListDataPager : DataPager
	{
		public string CssClass { get; set; }

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Ul; }
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			if (!string.IsNullOrEmpty(CssClass))
			{
				Attributes["class"] = CssClass;
			}

			base.AddAttributesToRender(writer);
		}
	}

	/// <summary>
	/// Override of <see cref="NextPreviousPagerField"/> that renders its controls wrapped in an li element.
	/// </summary>
	public class ListItemNextPreviousPagerField : NextPreviousPagerField
	{
		public string ListItemCssClass { get; set; }

		public override void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex)
		{
			if (container == null)
			{
				return;
			}

			RenderNonBreakingSpacesBetweenControls = false;

			var parent = container.Parent;

			parent.Controls.Remove(container);

			var overrideContainer = new ListItemDataPagerFieldItem(container, RenderDisabledButtonsAsLabels, ListItemCssClass);

			parent.Controls.Add(overrideContainer);

			base.CreateDataPagers(overrideContainer, startRowIndex, maximumRows, totalRowCount, fieldIndex);
		}
	}
	
	/// <summary>
	/// Override of <see cref="NumericPagerField"/> that renders its controls wrapped in an li element.
	/// </summary>
	public class ListItemNumericPagerField : NumericPagerField
	{
		public string ListItemCssClass { get; set; }

		public override void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex)
		{
			if (container == null)
			{
				return;
			}

			RenderNonBreakingSpacesBetweenControls = false;

			var parent = container.Parent;

			parent.Controls.Remove(container);

			var overrideContainer = new ListItemDataPagerFieldItem(container, false, ListItemCssClass);

			parent.Controls.Add(overrideContainer);

			base.CreateDataPagers(overrideContainer, startRowIndex, maximumRows, totalRowCount, fieldIndex);
		}
	}

	internal class ListItemDataPagerFieldItem : DataPagerFieldItem
	{
		public ListItemDataPagerFieldItem(DataPagerFieldItem inner, bool labelsAreDisabled, string listItemCssClass) : base(inner.PagerField, inner.Pager)
		{
			LabelsAreDisabled = labelsAreDisabled;
			ListItemCssClass = listItemCssClass;
		}

		protected bool LabelsAreDisabled { get; private set; }

		protected string ListItemCssClass { get; private set; }

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			var children = Controls.Cast<Control>().ToArray();

			foreach (var child in children)
			{
				Controls.Remove(child);

				var li = new HtmlGenericControl("li");

				AddCssClasses(li, ListItemCssClass);

				li.Controls.Add(child);

				var webControl = child as WebControl;

				if (webControl != null && !webControl.Enabled && (webControl is HyperLink || webControl is IButtonControl))
				{
					AddCssClasses(li, "disabled");
				}

				var label = child as Label;

				if (label != null)
				{
					AddCssClasses(li, LabelsAreDisabled ? "disabled" : "active");
				}

				Controls.Add(li);
			}

			base.RenderChildren(writer);
		}

		private static void AddCssClasses(HtmlControl control, params string[] classes)
		{
			var includingExistingClass = new[] { control.Attributes["class"] }.Union(classes);
			var attributeValue = string.Join(" ", includingExistingClass.Where(@class => !string.IsNullOrWhiteSpace(@class)));

			if (!string.IsNullOrWhiteSpace(attributeValue))
			{
				control.Attributes["class"] = attributeValue;
			}
		}
	}
}
