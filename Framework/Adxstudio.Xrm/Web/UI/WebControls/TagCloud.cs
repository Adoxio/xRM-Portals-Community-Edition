/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Renders a weighted tag cloud for all tags present on a site.
	/// </summary>
	[Description("Renders a weighted tag cloud for all tags present on a site")]
	[ToolboxData(@"<{0}:TagCloud runat=""server""></{0}:TagCloud>")]
	public class TagCloud : ListView
	{
		private string _weightCssClassPrefix;
		private int? _weights;

		/// <summary>
		/// Gets of sets the number of weight groups/tiers into which results will be divided.
		/// </summary>
		/// <remarks>
		/// The default value of this property is <see cref="TagCloudDataSource.DefaultNumberOfWeights"/>.
		/// </remarks>
		[Description("The number of weight groups/tiers into which results will be divided")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue(TagCloudDataSource.DefaultNumberOfWeights)]
		public int NumberOfWeights
		{
			get { return _weights.HasValue ? _weights.Value : TagCloudDataSource.DefaultNumberOfWeights; }
			set { _weights = value; }
		}

		/// <summary>
		/// Gets or sets the query string field that will be appended to the TagNavigateUrl for tag links.
		/// </summary>
		[Description("The URL used for tag links")]
		[Bindable(false)]
		[Category("Data")]
		public string TagNavigateUrl { get; set; }

		/// <summary>
		/// Gets or sets the query string field that will be appended to the TagNavigateUrl for tag links.
		/// </summary>
		[Description("The tag query string field that will be appended to the tag navigate URL")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue("tag")]
		public string TagQueryStringField { get; set; }

		/// <summary>
		/// Gets or sets the CSS class prefix to which the weight of a tag result item will be appended.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value defaults to "tag-weight-". So, for example, items returned by this control, by default,
		/// will have a CssClass property of "tag-weight-1", "tag-weight-2", etc.
		/// </para>
		/// </remarks>
		[Description("The CSS class prefix to which the weight of a tag result item will be appended")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue(TagCloudDataSource.DefaultWeightCssClassPrefix)]
		public string WeightCssClassPrefix
		{
			get { return string.IsNullOrEmpty(_weightCssClassPrefix) ? TagCloudDataSource.DefaultWeightCssClassPrefix : _weightCssClassPrefix; }
			set { _weightCssClassPrefix = value; }
		}

		protected override void OnInit(EventArgs args)
		{
			base.OnInit(args);

			if (DataSource == null)
			{
				DataSource = new TagCloudDataSource
				{
					NumberOfWeights = NumberOfWeights,
					SortDirection = SortDirection,
					SortExpression = SortExpression,
					WeightCssClassPrefix = WeightCssClassPrefix
				};
			}

			if (ItemTemplate == null && LayoutTemplate == null)
			{
				LayoutTemplate = new DefaultTagCloudLayoutTemplate(this);

				ItemTemplate = new DefaultTagCloudItemTemplate
				{
					TagNavigateUrl = TagNavigateUrl,
					TagQueryStringField = TagQueryStringField
				};
			}
		}

		/// <remarks>
		/// This control overrides this event handler to make a call to DataBind. This means that
		/// databinding always occurs implicitly for this control, unless this method is overridden in a
		/// deriving class.
		/// </remarks>	
		protected override void OnLoad(EventArgs args)
		{
			base.OnLoad(args);

			if (!Page.IsPostBack)
			{
				DataBind();
			}
		}

		protected override void OnPreRender(EventArgs args)
		{
			base.OnPreRender(args);

			if (Page.IsPostBack)
			{
				DataBind();
			}
		}

		protected override void LoadViewState(object savedState)
		{
			var state = (Pair)savedState;

			if (savedState == null)
			{
				base.LoadViewState(null);
			}
			else
			{
				base.LoadViewState(state.First);

				var parameters = state.Second as object[];

				if (parameters != null)
				{
					NumberOfWeights = (int)parameters[0];
					WeightCssClassPrefix = parameters[1] as string;
				}
			}
		}

		protected override object SaveViewState()
		{
			var state = new Pair
			{
				First = base.SaveViewState(),
				Second = new object[]
				{
					NumberOfWeights,
					WeightCssClassPrefix
				}
			};

			if ((state.First == null) && (state.Second == null))
			{
				return null;
			}

			return state;
		}

		public abstract class DataBoundTemplate : ITemplate
		{
			public abstract void InstantiateIn(Control container);

			protected static EventHandler OnDataBinding<TSender>(Action<TSender, Func<string, object>> action) where TSender : Control
			{
				return (sender, args) => OnDataBinding(sender, args, action);
			}

			protected static void OnDataBinding<TSender>(object sender, EventArgs args, Action<TSender, Func<string, object>> action) where TSender : Control
			{
				var control = sender as TSender;

				if (control == null) return;

				var container = control.NamingContainer as IDataItemContainer;

				if (container == null) return;

				var dataItem = container.DataItem;

				if (dataItem == null) return;

				action(control, evalString => DataBinder.Eval(dataItem, evalString));
			}
		}

		public class DefaultTagCloudItemTemplate : DataBoundTemplate
		{
			public string TagNavigateUrl { get; set; }

			public string TagQueryStringField { get; set; }

			public override void InstantiateIn(Control container)
			{
				var item = new HtmlGenericControl("li");

				container.Controls.Add(item);

				var tagHyperLink = new HyperLink();

				tagHyperLink.DataBinding += OnDataBinding<HyperLink>((hyperLink, eval) =>
				{
					var tagName = eval("Name") as string;

					if (string.IsNullOrEmpty(tagName))
					{
						hyperLink.NavigateUrl = TagNavigateUrl;

						return;
					}

					hyperLink.CssClass = "tag {0}".FormatWith(eval("CssClass"));
					hyperLink.Text = tagName;
					hyperLink.NavigateUrl = BuildNavigateUrl(tagName);
				});

				item.Controls.Add(tagHyperLink);
				item.Controls.Add(new HtmlGenericControl("span") { InnerHtml = "&ensp;" });
			}

			private string BuildNavigateUrl(string tagName)
			{
				if (TagNavigateUrl == null)
				{
					return null;
				}

				var queryPart = "{0}={1}".FormatWith(TagQueryStringField, tagName);

				return TagNavigateUrl.Contains("?")
					? TagNavigateUrl + "&" + queryPart
					: TagNavigateUrl + "?" + queryPart;
			}
		}

		public class DefaultTagCloudLayoutTemplate : ITemplate
		{
			private readonly TagCloud _tagCloud;

			public DefaultTagCloudLayoutTemplate(TagCloud tagCloud)
			{
				if (tagCloud == null) throw new ArgumentNullException("tagCloud");

				_tagCloud = tagCloud;
			}

			public void InstantiateIn(Control container)
			{
				var list = new HtmlGenericControl("ul");

				list.Attributes["class"] = "tag-cloud";

				container.Controls.Add(list);

				var itemPlaceholder = new HtmlGenericControl("li") { ID = _tagCloud.ItemPlaceholderID ?? "itemPlaceholder" };

				list.Controls.Add(itemPlaceholder);
			}
		}
	}
}
