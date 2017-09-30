/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Coordinates loading of script dependencies for controls that request XRM inline editing support.
	/// </summary>
	[NonVisualControl, DefaultProperty("DependencyScripts"), ParseChildren(true), PersistChildren(false)]
	public sealed class SiteEditingManager : WebControl
	{
		private ScriptReferenceCollection _dependencyScripts;
		private StyleReferenceCollection _dependencyStyles;
		private ScriptReferenceCollection _extensionScripts;
		private StyleReferenceCollection _extensionStyles;

		/// <summary>
		/// Gets a collection of framework dependency <see cref="ScriptReference">script references</see> to be loaded prior
		/// to the XRM inline editing scripts.
		/// </summary>
		/// <remarks>
		/// These scripts will only be loaded if XRM inline editing support is requested by another control on the page.
		/// </remarks>
		[DefaultValue((string)null), PersistenceMode(PersistenceMode.InnerProperty), Category("Behavior"), MergableProperty(false)]
		public ScriptReferenceCollection DependencyScripts
		{
			get
			{
				if (_dependencyScripts == null)
				{
					_dependencyScripts = new ScriptReferenceCollection();
				}

				return _dependencyScripts;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="StyleReference">style references</see> to be linked by the page if XRM inline
		/// editing support is request by a control on the page.
		/// </summary>
		/// <remarks>
		/// These styles will be loaded prior to any styles included by the framework.
		/// </remarks>
		[DefaultValue((string)null), PersistenceMode(PersistenceMode.InnerProperty), Category("Behavior"), MergableProperty(false)]
		public StyleReferenceCollection DependencyStyles
		{
			get
			{
				if (_dependencyStyles == null)
				{
					_dependencyStyles = new StyleReferenceCollection();
				}

				return _dependencyStyles;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="ScriptReference">script references</see> to be loaded after the XRM inline
		/// editing scripts.
		/// </summary>
		/// <remarks>
		/// These scripts will only be loaded if XRM inline editing support is requested by another control on the page.
		/// </remarks>
		[DefaultValue((string)null), PersistenceMode(PersistenceMode.InnerProperty), Category("Behavior"), MergableProperty(false)]
		public ScriptReferenceCollection ExtensionScripts
		{
			get
			{
				if (_extensionScripts == null)
				{
					_extensionScripts = new ScriptReferenceCollection();
				}

				return _extensionScripts;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="StyleReference">style references</see> to be linked by the page if XRM inline
		/// editing support is request by a control on the page.
		/// </summary>
		/// <remarks>
		/// These styles will be loaded after to any styles included by the framework.
		/// </remarks>
		[DefaultValue((string)null), PersistenceMode(PersistenceMode.InnerProperty), Category("Behavior"), MergableProperty(false)]
		public StyleReferenceCollection ExtensionStyles
		{
			get
			{
				if (_extensionStyles == null)
				{
					_extensionStyles = new StyleReferenceCollection();
				}

				return _extensionStyles;
			}
		}

		public string PortalName { get; set; }

		public void RegisterClientSideDependencies(WebControl control)
		{
			RegisterScripts(control, DependencyScripts);

			AddStyleReferencesToPage(control.Page, DependencyStyles);

			var provider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IRegisterClientSideDependenciesProvider>();

			provider.Register(control);

			RegisterScripts(control, ExtensionScripts);

			AddStyleReferencesToPage(control.Page, ExtensionStyles);
		}

		private static void AddStyleReferencesToPage(Page page, StyleReferenceCollection styles)
		{
			if (page.Header == null)
			{
				return;
			}

			foreach (var style in styles)
			{
				if (string.IsNullOrEmpty(style.Path))
				{
					continue;
				}

				var path = style.Path;

				if (ControlContainsStylesheetLink(page.Header, path))
				{
					continue;
				}

				var link = new HtmlLink { Href = path };

				link.Attributes["rel"] = "stylesheet";
				link.Attributes["type"] = "text/css";

				if (!string.IsNullOrEmpty(style.Media))
				{
					link.Attributes["media"] = style.Media;
				}

				page.Header.Controls.Add(link);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (DesignMode)
			{
				return;
			}

			var page = Page;

			if (GetCurrent(page) != null)
			{
				throw new InvalidOperationException("Only one instance of a {0} can be added to the page.".FormatWith(typeof(SiteEditingManager).Name));
			}

			page.Items[typeof(SiteEditingManager)] = this;
		}

		public static SiteEditingManager GetCurrent(Page page)
		{
			page.ThrowOnNull("page");

			return (page.Items[typeof(SiteEditingManager)] as SiteEditingManager);
		}

		private static void RegisterScripts(Control control, IEnumerable<ScriptReference> scripts)
		{
			if (!scripts.Any())
			{
				return;
			}

			var scriptManager = ScriptManager.GetCurrent(control.Page);

			if (scriptManager == null)
			{
				throw new InvalidOperationException("{0} requires an instance of ScriptManager to exist on the page.".FormatWith(typeof(SiteEditingManager).Name));
			}

			foreach (var script in scripts)
			{
				scriptManager.Scripts.Add(script);
			}
		}

		private static bool ControlContainsStylesheetLink(Control container, string stylesheetPath)
		{
			foreach (var control in container.Controls)
			{
				if (control is HtmlLink && (control as HtmlLink).Href == stylesheetPath)
				{
					return true;
				}
			}

			return false;
		}
	}

	[DefaultProperty("Path"), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
	public class StyleReference
	{
		private string _path;

		[Category("Behavior"), DefaultValue("")]
		public virtual string Media { get; set; }

		[Category("Behavior"), UrlProperty("*.css"), DefaultValue("")]
		public virtual string Path
		{
			get { return _path ?? string.Empty; }
			set { _path = value; }
		}
	}

	[AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
	public class StyleReferenceCollection : Collection<StyleReference> { }
}
