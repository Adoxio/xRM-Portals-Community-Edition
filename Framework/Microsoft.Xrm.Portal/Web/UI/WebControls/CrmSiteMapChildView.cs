/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Displays the child <see cref="SiteMapNode"/>s of a given starting <see cref="SiteMapNode"/>, using user-defined templates.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Though this control inherits from <see cref="ListView"/>, which supports explicit custom data sources, this control only
	/// uses its own implicit data source. Attempting to set the data source of this control externally will cause exceptions.
	/// </para>
	/// <para>
	/// This control supports ADXSTUDIO XRM front-side editing, in the form of an interface to update the display orders of the
	/// rendered children. This interface will appear if the current user has <see cref="CrmEntityRight.Change"/> access for the
	/// <see cref="CrmEntity"/> associated with the starting <see cref="CrmSiteMapNode"/>.
	/// </para>
	/// </remarks>
	public class CrmSiteMapChildView : ListView, IEditableCrmEntityControl
	{
		private bool _editable = true;

		/// <summary>
		/// Gets or sets the base URI of the data service to be used for front-side editing functionality
		/// provided by this control. Set to use a data service other than the system global/default service.
		/// </summary>
		public string CmsServiceBaseUri { get; set; }

		/// <summary>
		/// Gets or sets the object from which the data-bound control retrieves its data items.
		/// </summary>
		/// <remarks>
		/// Setting this property is not supported by the control, and will raise a <see cref="NotSupportedException"/>.
		/// </remarks>
		public override object DataSource
		{
			get { return base.DataSource; }
			set { throw new NotSupportedException("This control uses an implicit data source and does not support setting an explicit data source."); }
		}

		/// <summary>
		/// Gets or sets the ID of the control from which the data-bound control retrieves its data items.
		/// </summary>
		/// <remarks>
		/// Setting this property is not supported by the control, and will raise a <see cref="NotSupportedException"/>.
		/// </remarks>
		public override string DataSourceID
		{
			get { return base.DataSourceID; }
			set { throw new NotSupportedException("This control uses an implicit data source and does not support setting an explicit data source."); }
		}

		/// <summary>
		/// Gets or sets a Boolean value indication whether or not this property value will be inline editable
		/// (provided the user has edit permission, and no other properties have been set on this control which
		/// disable inline editing support).
		/// </summary>
		public virtual bool Editable
		{
			get { return _editable; }
			set { _editable = value; }
		}

		/// <summary>
		/// Gets or sets the custom content for the root container.
		/// </summary>
		public override ITemplate LayoutTemplate
		{
			get { return base.LayoutTemplate; }
			set
			{
				if (value != null)
				{
					base.LayoutTemplate = new EditableLayoutTemplate(this, value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of the site map provider that the control binds to.
		/// </summary>
		[DefaultValue("")]
		public string SiteMapProvider
		{
			get { return (ViewState["SiteMapProvider"] as string) ?? string.Empty; }
			set { ViewState["SiteMapProvider"] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the control will use the current site map node as the starting point for data retrieval.
		/// </summary>
		/// <remarks>
		/// Unless a <see cref="StartingNodeUrl"/> is provided, this value default to true.
		/// </remarks>
		[DefaultValue(true)]
		public bool StartFromCurrentNode
		{
			get
			{
				var value = ViewState["StartFromCurrentNode"];

				return (value != null) ? ((bool)value) : true;
			}
			set { ViewState["StartFromCurrentNode"] = value; }
		}

		/// <summary>
		/// Gets or sets a negative integer offset from the staring node that determines the root node whose child nodes will be bound.
		/// </summary>
		/// <remarks>
		/// This control only supports negative offets--values greater than 0 are not supported.
		/// </remarks>
		[DefaultValue(0)]
		public int StartingNodeOffset
		{
			get
			{
				var value = ViewState["StartingNodeOffset"] as int?;

				return value == null ? 0 : value.Value;
			}
			set
			{
				if (value > 0)
				{
					throw new ArgumentException("Starting node offsets of greater than 0 are not supported by this control.", "value");
				}

				ViewState["StartingNodeOffset"] = new int?(value);
			}
		}

		/// <summary>
		/// Gets or sets the URL of the staring node that determines the root node whose child nodes will be bound.
		/// </summary>
		[UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string StartingNodeUrl { get; set; }

		/// <summary>
		/// Gets or sets the name of the portal configuration that the control binds to.
		/// </summary>
		[DefaultValue("")]
		public string PortalName
		{
			get { return (ViewState["PortalName"] as string) ?? string.Empty; }
			set { ViewState["PortalName"] = value; }
		}

		/// <summary>
		/// Gets a Boolean indicating whether or not the current user has Change permission for the entity
		/// associated with the starting site map node of this control.
		/// </summary>
		protected virtual bool HasEditPermission
		{
			get
			{
				var startingNode = GetStartingNode() as CrmSiteMapNode;

				if (startingNode == null || startingNode.Entity == null)
				{
					return false;
				}

				var portalName = GetPortalName();
				var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);

				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
				var context = portal.ServiceContext;

				var entity = context.MergeClone(startingNode.Entity);

				return securityProvider.TryAssert(context, entity, CrmEntityRight.Change);
			}
		}

		/// <summary>
		/// Gets the starting <see cref="SiteMapNode"/> of this control.
		/// </summary>
		protected SiteMapNode GetStartingNode()
		{
			var provider = GetProvider();

			if (provider == null)
			{
				throw new InvalidOperationException(@"Unable to find site map provider ""{0}"".".FormatWith(SiteMapProvider));
			}

			var selectedNode = string.IsNullOrEmpty(StartingNodeUrl) ? provider.CurrentNode : provider.FindSiteMapNode(StartingNodeUrl);
			var startingNode = selectedNode;

			var offset = StartingNodeOffset;

			if (offset < 0)
			{
				while (offset < 0 && startingNode != null)
				{
					var parentNode = startingNode.ParentNode;

					if (parentNode == null)
					{
						break;
					}

					startingNode = parentNode;

					offset++;
				}
			}

			return startingNode;
		}

		protected override void OnLoad(EventArgs e)
		{
			var dataSource = new SiteMapDataSource { ShowStartingNode = false, StartingNodeOffset = StartingNodeOffset };

			if (!string.IsNullOrEmpty(SiteMapProvider))
			{
				dataSource.SiteMapProvider = SiteMapProvider;
			}

			if (string.IsNullOrEmpty(StartingNodeUrl) && !StartFromCurrentNode)
			{
				throw new InvalidOperationException("If not starting from current node, a StartingNodeUrl must be provided.");
			}

			if (!string.IsNullOrEmpty(StartingNodeUrl))
			{
				dataSource.StartingNodeUrl = StartingNodeUrl;
			}
			else
			{
				dataSource.StartFromCurrentNode = StartFromCurrentNode;
			}

			base.DataSource = dataSource;

			DataBind();

			base.OnLoad(e);
		}

		public class EditableLayoutTemplate : ITemplate
		{
			private readonly ITemplate _innerTemplate;
			private readonly CrmSiteMapChildView _owner;

			public EditableLayoutTemplate(CrmSiteMapChildView owner, ITemplate innerTemplate)
			{
				owner.ThrowOnNull("owner");
				innerTemplate.ThrowOnNull("innerTemplate");

				_owner = owner;
				_innerTemplate = innerTemplate;
			}

			public void InstantiateIn(Control container)
			{
				var editableWrapper = new HtmlGenericControl("div");

				if (_owner.Editable)
				{
					editableWrapper.Attributes["class"] = "xrm-entity xrm-editable-sitemapchildren";
				}

				container.Controls.Add(editableWrapper);

				_innerTemplate.InstantiateIn(editableWrapper);

				var startingNode = _owner.GetStartingNode();

				if (_owner.Editable && _owner.HasEditPermission && startingNode != null)
				{
					var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(_owner.PortalName).GetDependency<ICrmEntityEditingMetadataProvider>();

					metadataProvider.AddSiteMapNodeMetadata(_owner.PortalName, _owner, editableWrapper, startingNode);

					_owner.RegisterClientSideDependencies();
				}
			}
		}

		private SiteMapProvider GetProvider()
		{
			return string.IsNullOrEmpty(SiteMapProvider) ? SiteMap.Provider : SiteMap.Providers[SiteMapProvider];
		}

		private string GetPortalName()
		{
			if (!string.IsNullOrWhiteSpace(PortalName)) return PortalName;

			var provider = GetProvider() as CrmSiteMapProviderBase;
			if (provider != null) return provider.PortalName;

			return PortalName;
		}
	}
}
