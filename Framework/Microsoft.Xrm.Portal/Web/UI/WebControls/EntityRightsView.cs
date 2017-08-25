/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Templated <see cref="System.Web.UI.WebControls.WebControl"/> to render alternate views based on whether
	/// the current user has specified <see cref="Rights"/> (<see cref="CrmEntityRight"/>) over a given
	/// <see cref="Entity"/>. If no entity is specified the NoRightsTemplate will be rendered.
	/// </summary>
	/// <example>
	/// <code>
	///	<![CDATA[
	/// <adx:EntityRightsView Rights="Change" runat="server">
	///		<RightsTemplate>
	///			<p>You have permissions to change this entity.</p>
	///		</RightsTemplate>
	///		<NoRightsTemplate>
	///			<p>You do not have permissions to change this entity.</p>
	///		</NoRightsTemplate>
	/// </adx:EntityRightsView>
	/// ]]>
	/// </code>
	/// </example>
	[Description("Templated control to render alternate views based on whether the current user has specified rights over a given entity.")]
	[ToolboxData(@"<{0}:EntityRightsView runat=""server""></{0}:EntityRightsView>")]
	[ParseChildren(true)]
	[PersistChildren(false)]
	[DefaultProperty("CurrentView")]
	[DefaultEvent("ViewChanged")]
	[Bindable(false)]
	[Themeable(true)]
	[Designer(typeof(EntityRightsViewDesigner))]
	public sealed class EntityRightsView : WebControl, INamingContainer
	{
		private int _templateIndex;
		private const int NoRightsTemplateIndex = 0;
		private const int RightsTemplateIndex = 1;
		private static readonly object EventViewChanged = new object();
		private static readonly object EventViewChanging = new object();

		public string PortalName { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Entity"/> to be used the target of the rights assertion.
		/// </summary>
		[Description("The entity to be used the target of the rights assertion")]
		[Browsable(false)]
		[DefaultValue(null)]
		public Entity Entity { get; set; }

		/// <summary>
		/// The <see cref="CrmEntityRight"/> to be asserted against the target entity, in order to
		/// determine whether or not this control will render its contents.
		/// </summary>
		[Description("The entity rights to be asserted against the target entity")]
		[Category("Data")]
		[Browsable(true)]
		[DefaultValue(CrmEntityRight.Read)]
		public CrmEntityRight Rights { get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[Browsable(false)]
		[DefaultValue((string)null)]
		[TemplateContainer(typeof(EntityRightsView))]
		public ITemplate NoRightsTemplate { get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[Browsable(false)]
		[DefaultValue((string)null)]
		[TemplateContainer(typeof(EntityRightsView))]
		public ITemplate RightsTemplate { get; set; }

		public bool HasRights
		{
			get
			{
				try
				{
					var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);
					var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
					var context = portal.ServiceContext;

					return securityProvider.TryAssert(context, Entity, Rights);
				}
				catch (SecurityException)
				{
					return false;
				}
			}
		}

		public event EventHandler ViewChanged
		{
			add
			{
				Events.AddHandler(EventViewChanged, value);
			}
			remove
			{
				Events.RemoveHandler(EventViewChanged, value);
			}
		}

		public event EventHandler ViewChanging
		{
			add
			{
				Events.AddHandler(EventViewChanging, value);
			}
			remove
			{
				Events.RemoveHandler(EventViewChanging, value);
			}
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();

			var page = Page;

			if (((page != null) && !page.IsPostBack) && !DesignMode)
			{
				_templateIndex = GetTemplateIndex();
			}

			var templateIndex = TemplateIndex;

			ITemplate anonymousTemplate;

			switch (templateIndex)
			{
				case NoRightsTemplateIndex:

					anonymousTemplate = NoRightsTemplate;

					break;

				case RightsTemplateIndex:

					anonymousTemplate = RightsTemplate;

					break;

				default:

					anonymousTemplate = NoRightsTemplate;

					break;
			}

			if (anonymousTemplate == null) return;

			var container = new Control();

			if (DesignMode)
			{
				anonymousTemplate.InstantiateIn(container);

				Controls.Add(container);

				return;
			}

			if (HasRights)
			{
				if (RightsTemplate != null)
				{
					RightsTemplate.InstantiateIn(container);
				}
			}
			else
			{
				if (NoRightsTemplate != null)
				{
					NoRightsTemplate.InstantiateIn(container);
				}
			}

			Controls.Add(container);
		}

		public override void DataBind()
		{
			OnDataBinding(EventArgs.Empty);

			EnsureChildControls();

			DataBindChildren();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Focus()
		{
			throw new NotSupportedException("NoFocusSupport");
		}

		private int GetTemplateIndex()
		{
			if ((DesignMode || (Page == null)) || !Page.Request.IsAuthenticated)
			{
				return NoRightsTemplateIndex;
			}

			return RightsTemplateIndex;
		}

		protected override void LoadControlState(object savedState)
		{
			if (savedState == null) return;

			var pair = (Pair)savedState;

			if (pair.First != null)
			{
				base.LoadControlState(pair.First);
			}

			if (pair.Second != null)
			{
				_templateIndex = (int)pair.Second;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (Page == null) return;

			Page.RegisterRequiresControlState(this);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			TemplateIndex = GetTemplateIndex();

			EnsureChildControls();
		}

		private void OnViewChanged(EventArgs e)
		{
			var handler = (EventHandler)Events[EventViewChanged];

			if (handler == null) return;

			handler(this, e);
		}

		private void OnViewChanging(EventArgs e)
		{
			var handler = (EventHandler)Events[EventViewChanging];

			if (handler == null) return;

			handler(this, e);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			EnsureChildControls();

			base.Render(writer);
		}

		protected override object SaveControlState()
		{
			var x = base.SaveControlState();

			if ((x == null) && (_templateIndex == 0))
			{
				return null;
			}

			object y = null;

			if (_templateIndex != 0)
			{
				y = _templateIndex;
			}

			return new Pair(x, y);
		}

		protected override void SetDesignModeState(IDictionary data)
		{
			if (data == null) return;

			var obj2 = data["TemplateIndex"];

			if (obj2 == null) return;

			TemplateIndex = (int)obj2;

			ChildControlsCreated = false;
		}
        
		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();

				return base.Controls;
			}
		}

		[Browsable(true)]
		public override bool EnableTheming
		{
			get
			{
				return base.EnableTheming;
			}
			set
			{
				base.EnableTheming = value;
			}
		}
        
		[Browsable(true)]
		public override string SkinID
		{
			get
			{
				return base.SkinID;
			}
			set
			{
				base.SkinID = value;
			}
		}

		private int TemplateIndex
		{
			get
			{
				return _templateIndex;
			}
			set
			{
				if (value == TemplateIndex) return;

				OnViewChanging(EventArgs.Empty);

				_templateIndex = value;

				ChildControlsCreated = false;

				OnViewChanged(EventArgs.Empty);
			}
		}		
	}
}
