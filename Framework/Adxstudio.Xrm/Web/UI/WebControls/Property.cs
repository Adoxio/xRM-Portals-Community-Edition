/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Renders the value of an <see cref="Entity"/>'s property.
	/// </summary>
	[ToolboxData("<{0}:Property runat=\"server\"></{0}:Property>")]
	public class Property : EditableCrmEntityDataBoundControl, ITextControl
	{
		public const string HtmlEditType = "html";
		public const string PlaintextEditType = "text";

		private string _editType;
		private Lazy<HtmlHelper> _html;
		private bool? _liquidEnabled;
		private bool _literal;
		
		/// <summary>
		/// Gets or sets a default text string to be rendered if the property value(s) targeted by this control
		/// is(are) null or empty.
		/// </summary>
		public virtual string DefaultText { get; set; }

		/// <summary>
		/// Gets or sets the string identifier of the type of editing interface to provide for this
		/// property (e.g., "html" or "text").
		/// </summary>
		public virtual string EditType
		{
			get { return _editType ?? PlaintextEditType; }
			set { _editType = value; }
		}

		/// <summary>
		/// (Optional) Gets or sets the html tag type used to contain the snippet. If this is not set, the
		/// control will render a div when EditType="html", or a span when EditType="text".
		/// </summary>
		public virtual HtmlTextWriterTag HtmlTag { get; set; }

		public virtual bool LiquidEnabled
		{
			get { return _liquidEnabled.GetValueOrDefault(LiquidExtensions.LiquidEnabledDefault); }
			set { _liquidEnabled = value; }
		}

		/// <summary>
		/// Gets or sets a Boolean value indicating whether or not this control should render as a literal
		/// value, i.e., only the raw text value of the bound property will be rendered, with no surrounding
		/// DOM nodes.
		/// </summary>
		/// <remarks>
		/// Setting this value to true disables any inline editing support for the bound property.
		/// </remarks>
		public virtual bool Literal
		{
			get { return _literal || (ControlIsInPageHeader && ItemTemplate == null); }
			set { _literal = value; }
		}

		public virtual string Text
		{
			get
			{
				var text = ViewState["Text"];

				if (text != null)
				{
					return (string)text;
				}

				return string.Empty;
			}

			set
			{
				if (HasControls())
				{
					Controls.Clear();
				}

				ViewState["Text"] = value;
			}
		}

		protected bool ControlIsInPageHeader
		{
			get
			{
				var parent = Parent;

				while (parent != null)
				{
					if (parent == Page.Header)
					{
						return true;
					}

					parent = parent.Parent;
				}

				return false;
			}
		}

		protected HtmlHelper Html
		{
			get
			{
				var portalViewPage = Page as PortalViewPage;

				return portalViewPage == null ? _html.Value : portalViewPage.Html;
			}
		}

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				if (HtmlTag != HtmlTextWriterTag.Unknown)
				{
					return HtmlTag;
				}

				// Use a div container element in the case that this control is marked as HTML.
				return string.Equals(EditType, HtmlEditType, StringComparison.InvariantCultureIgnoreCase)
					? HtmlTextWriterTag.Div
					: base.TagKey;
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if (HasControls())
			{
				base.AddParsedSubObject(obj);
			}
			else if (obj is LiteralControl)
			{
				Text = ((LiteralControl)obj).Text;
			}
			else
			{
				var text = Text;

				if (text.Length != 0)
				{
					Text = string.Empty;

					base.AddParsedSubObject(new LiteralControl(text));
				}

				base.AddParsedSubObject(obj);
			}
		}

		protected virtual void CreateControlsForInlineEditing(Entity entity, string propertyName, string value)
		{
			var editablePropertyCssClass = "xrm-attribute xrm-editable-{0}".FormatWith(EditType.ToLowerInvariant());

			CssClass = string.IsNullOrEmpty(CssClass) ? editablePropertyCssClass : "{0} {1}".FormatWith(CssClass, editablePropertyCssClass);

			var valueContainer = new HtmlGenericControl(TagName) { InnerHtml = value };

			valueContainer.Attributes["class"] = "xrm-attribute-value";

			Controls.Add(valueContainer);

			if (entity == null || string.IsNullOrEmpty(propertyName))
			{
				return;
			}

			if (HasEditPermission(entity))
			{
				Attributes["data-encoded"] = HtmlEncode ? "true" : "false";
				Attributes["data-liquid"] = LiquidEnabled ? "true" : "false";

				var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityEditingMetadataProvider>();

				metadataProvider.AddAttributeMetadata(PortalName, this, this, entity, propertyName, GetEditDisplayName(entity, propertyName));

				this.RegisterClientSideDependencies();
			}
		}

		protected virtual string GetEditDisplayName(Entity entity, string propertyName)
		{
			using (var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName))
			{
				return serviceContext.GetEntityPrimaryNameWithAttributeLabel(entity, propertyName)
					?? "{0}.{1}".FormatWith(entity.LogicalName, propertyName);
			}
		}

		protected override void LoadViewState(object savedState)
		{
			if (savedState == null)
			{
				return;
			}

			base.LoadViewState(savedState);

			var text = ViewState["Text"] as string;

			if (text != null)
			{
				Text = text;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			_html = PortalViewPage.GetLazyHtmlHelper(PortalName, Context.Request.RequestContext, Context.Response);

			base.OnInit(e);
		}

		protected override void PerformDataBindingOfCrmEntityProperty(Entity entity, string propertyName, string value)
		{
			var text = string.IsNullOrEmpty(value) ? DefaultText : value;

			if (LiquidEnabled && !string.IsNullOrEmpty(text))
			{
				text = Html.Liquid(text);
			}

			// If this flag is set, just set up to render the raw text value. This disables inline editing.
			if (Literal)
			{
				Text = text;

				return;
			}

			if (string.IsNullOrEmpty(value))
			{
				CssClass = "{0} no-value".FormatWith(CssClass);
			}

			// Only the Literal/Text path supports viewstate management, disable it otherwise.
			EnableViewState = false;

			// If there's an ItemTemplate, bind to that. Sorry, this also means inline editing is disabled.
			if (ItemTemplate != null)
			{
				CreateItem(this, 0, ListItemType.Item, true, text);

				return;
			}

			if (!Editable)
			{
				Controls.Add(new Literal { Text = text });

				return;
			}

			// Otherwise, render the DOM necessary for inline editing support.
			CreateControlsForInlineEditing(entity, propertyName, text);
		}

		protected override void PerformDataBindingOfNonCrmEntity(object value)
		{
			if (Literal)
			{
				Text = value == null ? string.Empty : value.ToString();

				return;
			}

			// Only the Literal/Text path supports viewstate management, disable it otherwise.
			EnableViewState = false;

			// If there's an ItemTemplate, bind to that. Sorry, this also means inline editing is disabled.
			if (ItemTemplate != null)
			{
				CreateItem(this, 0, ListItemType.Item, true, value);

				return;
			}

			Controls.Add(new Literal { Text = value == null ? string.Empty : value.ToString() });
		}

		protected override void Render(HtmlTextWriter writer)
		{
			if (HasControls())
			{
				base.Render(writer);
			}
			else
			{
				writer.Write(Text);
			}
		}
	}
}


