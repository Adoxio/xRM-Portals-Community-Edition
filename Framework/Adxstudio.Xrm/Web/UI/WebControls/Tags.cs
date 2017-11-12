/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[ToolboxData("<{0}:TagRepeater runat=server></{0}:TagRepeater>")]
	public class Tags : Repeater
	{
		#region Defaults
		private string deleteTagAlternateText = "(x)";
		private string deleteTagCssClass = "remove-tag";
		private string deleteTagToolTip = ResourceManager.GetString("Remove_Tag_Tool_Tip_Text");
		private string newTagCssClass = "new-tag";
		private string newTagTitle = ResourceManager.GetString("New_Tag_Title_Text");
		private string newTagButtonName = ResourceManager.GetString("Create_Text");
		private string newTagButtonCssClass = "create-tag";
		private string readOnlyMessage = string.Empty;
		private int autocompleteMaxItemsToShow = 10;
		#endregion

		private ITaggable _taggableItem;
		
		public string DeleteTagAlternateText
		{
			get { return deleteTagAlternateText; }
			set { deleteTagAlternateText = value; }
		}

		public string DeleteTagCssClass
		{
			get { return deleteTagCssClass; }
			set { deleteTagCssClass = value; }
		}

		public string DeleteTagToolTip
		{
			get { return deleteTagToolTip; }
			set { deleteTagToolTip = value; }
		}

		public string NewTagCssClass
		{
			get { return newTagCssClass; }
			set { newTagCssClass = value; }
		}

		public string NewTagTitle
		{
			get { return newTagTitle; }
			set { newTagTitle = value; }
		}

		public string NewTagButtonName
		{
			get { return newTagButtonName; }
			set { newTagButtonName = value; }
		}

		public string NewTagButtonCssClass
		{
			get { return newTagButtonCssClass; }
			set { newTagButtonCssClass = value; }
		}

		public string ReadOnlyMessage
		{
			get { return readOnlyMessage; }
			set { readOnlyMessage = value; }
		}

		public int AutocompleteMaxItemsToShow
		{
			get { return autocompleteMaxItemsToShow; }
			set { autocompleteMaxItemsToShow = value; }
		}
		
		public string Title { get; set; }
	
		public bool IsReadOnly { get; set; }

		private bool isDeleteEnable;
		public bool IsDeleteEnable
		{
			get
			{
				if (IsReadOnly)
				{
					return false;
				}
				return isDeleteEnable;
			}
			set
			{
				isDeleteEnable = value;
			}
		}
		
		public string AutoCompleteServiceUrl { get; set; }

		public string NavigateUrl { get; set; }

		public string DeleteTagImageUrl { get; set; }

		public ITaggable TaggableItem
		{
			get
			{
				return _taggableItem ?? (_taggableItem = GetTaggable());
			}
		}

		private ITaggable GetTaggable()
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			if (portal.Entity.LogicalName == "adx_webpage")
			{
				return new WebPageTaggingAdapter(portal.Entity, PortalName);
			}

			if (portal.Entity.LogicalName == "adx_event")
			{
				return new EventTaggingAdapter(portal.Entity, PortalName);
			}

			if (portal.Entity.LogicalName == "adx_communityforumthread")
			{
				return new ForumThreadTaggingAdapter(portal.Entity, PortalName);
			}

			return null;
		}

		public string DefaultItemTemplateUrl { get; private set; }

		public string CssClass { get; set; }

		public string PortalName { get; set; }
		
		private int DataSourceCount
		{
			get
			{
				return (TaggableItem.Tags).Count();
			}
		}

		private TextBox newTagTextBox;

		/// <summary>
		/// Creates a control hierarchy, with or without the specified data source.
		/// </summary>
		/// <param name="useDataSource">Indicates whether to use the specified data source.</param>
		protected override void CreateControlHierarchy(bool useDataSource)
		{
			if (IsReadOnly && DataSourceCount < 1)
			{
				return;
			}
			if (HeaderTemplate == null)
			{
				HeaderTemplate = new CompiledTemplateBuilder(BuildTemplate);
			}
			if (ItemTemplate == null)
			{
				ItemTemplate = !string.IsNullOrEmpty(DefaultItemTemplateUrl)
					? Page.LoadTemplate(DefaultItemTemplateUrl)
					: new CompiledTemplateBuilder(BuildTemplate);
			}
			if (FooterTemplate == null)
			{
				FooterTemplate = new CompiledTemplateBuilder(BuildTemplate);
			}
			base.CreateControlHierarchy(useDataSource);
		}

		/// <summary>
		/// Builds the template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildTemplate(System.Web.UI.Control container)
		{
			var repeaterItem = container as RepeaterItem;

			if (repeaterItem.ItemType == ListItemType.Item)
			{
				BuildItemTemplate(repeaterItem);
			}
			else if (repeaterItem.ItemType == ListItemType.AlternatingItem)
			{
				BuildAlternatingItemTemplate(repeaterItem);
			}
			else if (repeaterItem.ItemType == ListItemType.EditItem)
			{
				BuildEditItemTemplate(repeaterItem);
			}
			else if (repeaterItem.ItemType == ListItemType.SelectedItem)
			{
				BuildSelectedItemTemplate(repeaterItem);
			}
			else if (repeaterItem.ItemType == ListItemType.Header)
			{
				BuildHeaderTemplate(repeaterItem);
			}
			else if (repeaterItem.ItemType == ListItemType.Footer)
			{
				BuildFooterTemplate(repeaterItem);
			}
			else if (repeaterItem.ItemType == ListItemType.Pager)
			{
				BuildPagerTemplate(repeaterItem);
			}
		}

		/// <summary>
		/// Builds the header template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildHeaderTemplate(IDataItemContainer container)
		{
			if (!string.IsNullOrEmpty(Title))
			{
				var title = new HtmlGenericControl("span");
				title.DataBinding += (sender, args) =>
				{
					title.InnerText = Title;
					title.Attributes.Add("class", "list-title");
				};
				(container as Control).Controls.Add(title);
			}
			var ulOpen = new Literal();
			ulOpen.DataBinding += (sender, args) =>
			{
				ulOpen.Text = "<ul class='" + CssClass + "' id ='" + ID + "'>";
			};
			(container as Control).Controls.Add(ulOpen);
		}

		/// <summary>
		/// Builds the footer template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildFooterTemplate(IDataItemContainer container)
		{
			var ulClose = new Literal();
			ulClose.DataBinding += (sender, args) => ulClose.Text = "</ul>";

			(container as Control).Controls.Add(ulClose);
			if (!IsReadOnly)
			{
				var div = new HtmlGenericControl("span");
				div.DataBinding += (sender, args) =>
				{
					div.InnerText = NewTagTitle;
					div.Attributes.Add("class", "box-title");
				};
				(container as Control).Controls.Add(div);
				
				newTagTextBox = new TextBox();
				newTagTextBox.DataBinding += (sender, args) =>
				{
					newTagTextBox.ID = "NewTagName";
					newTagTextBox.CssClass = NewTagCssClass;
					newTagTextBox.AutoCompleteType = AutoCompleteType.None;
				};
				(container as Control).Controls.Add(newTagTextBox);
				
				var createButton = new Button();
				createButton.DataBinding += (sender, args) => 
				{
					createButton.ID += createButton;
					createButton.Text = NewTagButtonName;
					createButton.CssClass = NewTagButtonCssClass;
					createButton.Click += CreateNewTag;
				};
				(container as Control).Controls.Add(createButton);
			}
			else
			{
				if (!string.IsNullOrEmpty(ReadOnlyMessage))
				{
					var div = new HtmlGenericControl("span");
					div.DataBinding += (sender, args) => 
					{
						div.InnerText = ReadOnlyMessage;
						div.Attributes.Add("class", "read-only-message");
					};
					(container as Control).Controls.Add(div);
				}
			}
		}

		/// <summary>
		/// Builds the pager template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildPagerTemplate(IDataItemContainer container)
		{ }

		/// <summary>
		/// Builds the alternating item template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildAlternatingItemTemplate(IDataItemContainer container)
		{
			BuildItemTemplate(container);
		}

		/// <summary>
		/// Builds the edit item template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildEditItemTemplate(IDataItemContainer container)
		{
			BuildItemTemplate(container);
		}

		/// <summary>
		/// Builds the selected item template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected virtual void BuildSelectedItemTemplate(IDataItemContainer container)
		{
			BuildItemTemplate(container);
		}

		/// <summary>
		/// Gets the data item.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <returns></returns>
		protected static ITagInfo GetDataItem(IDataItemContainer container)
		{
			return (container.DataItem as Entity).GetTagInfo();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Tags"/> class.
		/// </summary>
		public Tags()
		{
			Init += TagRepeater_Init;
			PreRender += TagRepeater_PreRender;
		}

		/// <summary>
		/// Handles the Init event of the TagRepeater control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void TagRepeater_Init(object sender, EventArgs e)
		{
			UpdateData();
		}

		/// <summary>
		/// Handles the PreRender event of the TagRepeater control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void TagRepeater_PreRender(object sender, EventArgs e)
		{
			RegisterAutocomplete();
		}

		/// <summary>
		/// Create js function "registerAutocomplete"
		/// </summary>
		private void RegisterAutocomplete()
		{
			if (!(IsReadOnly || string.IsNullOrEmpty(AutoCompleteServiceUrl)))
			{
				var autoCompleteScript = new StringBuilder();
				autoCompleteScript.Append("$(document).ready(function() {registerAutocomplete();});");
				autoCompleteScript.Append("function registerAutocomplete() {");
				autoCompleteScript.Append("$('input." + NewTagCssClass + "').autocomplete('" + AutoCompleteServiceUrl + "',");
				autoCompleteScript.Append("{matchContains:1,autoFill:true,maxItemsToShow:" + AutocompleteMaxItemsToShow + "}");
				autoCompleteScript.Append(");}");
				Page.ClientScript.RegisterStartupScript(this.GetType(), "JSScript", autoCompleteScript.ToString(), true);
			}
		}

		/// <summary>
		/// Builds the item template.
		/// </summary>
		/// <param name="container">The container.</param>
		protected void BuildItemTemplate(IDataItemContainer container)
		{
			string liClass = string.Empty;
			if (container.DataItemIndex == 0)
			{
				liClass = "first";
			}
			if (container.DataItemIndex == DataSourceCount - 1)
			{
				liClass += " last";
			}
			
			var liOpen = new Literal();
			liOpen.DataBinding += (sender, args) =>
			{
				if (!string.IsNullOrEmpty(liClass))
				{
					liOpen.Text = "<li class='" + liClass.Trim() + "'>";
				}
				else
				{
					liOpen.Text = "<li>";
				}
			};
			(container as Control).Controls.Add(liOpen);
			
			var link = new HyperLink();
			link.DataBinding += (sender, args) =>
			{
				link.Text = "{0}".FormatWith(GetDataItem(container).Name);
				link.NavigateUrl = NavigateUrl + "?Name=" + GetDataItem(container).Name;
			};
			(container as Control).Controls.Add(link);
			
			if (IsDeleteEnable)
			{
				var deleteButton = new ImageButton();
				deleteButton.DataBinding += (sender, args) =>
				{
					deleteButton.Click += DeleteTag;
					deleteButton.CssClass = DeleteTagCssClass;
					deleteButton.AlternateText = DeleteTagAlternateText;
					deleteButton.ImageUrl = DeleteTagImageUrl;
					deleteButton.CommandArgument = GetDataItem(container).Name;
					deleteButton.ToolTip = DeleteTagToolTip;
					deleteButton.OnClientClick = "javascript:return confirm('Are you sure you want to delete this tag?');return false;";
					
				};
				(container as Control).Controls.Add(deleteButton);
			}
			
			var liClose = new Literal();
			liClose.DataBinding += (sender, args) => liClose.Text = "</li>";
			(container as Control).Controls.Add(liClose);
		}

		/// <summary>
		/// Creates the new tag.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void CreateNewTag(object sender, EventArgs e)
		{
			if (newTagTextBox.Text != string.Empty)
			{
				string[] tags = newTagTextBox.Text.Split(',');
				foreach (string s in tags)
				{
					if (s != string.Empty)
					{
						TaggableItem.AddTag(s.Trim());
					}
				}
				UpdateData();
			}
		}

		/// <summary>
		/// Deletes the tag.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void DeleteTag(object sender, EventArgs e)
		{
			var tag = sender as ImageButton;
			if (tag != null)
			{
				TaggableItem.RemoveTag(tag.CommandArgument);
				UpdateData();
			}
		}

		protected void UpdateData()
		{
			DataSource = TaggableItem.Tags.ToList().OrderBy(t => t.GetAttributeValue<string>("adx_name"));
			
			DataBind();
		}
	}
}
