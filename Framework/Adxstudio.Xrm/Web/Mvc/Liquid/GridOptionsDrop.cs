/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class GridOptionsDrop : PortalDrop
	{
		private readonly Lazy<string> _loadingMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _errorMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _accessDeniedMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _emptyMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);

		public GridOptionsDrop(IPortalLiquidContext portalLiquidContext, GridOptions options, int languageCode)
			: base(portalLiquidContext)
		{
			if (options == null) return;
			CssClass = options.CssClass;
			GridCssClass = options.GridCssClass;
			_loadingMessage = Localization.CreateLazyLocalizedString(options.LoadingMessage, languageCode);
			_accessDeniedMessage = Localization.CreateLazyLocalizedString(options.AccessDeniedMessage, languageCode);
			_errorMessage = Localization.CreateLazyLocalizedString(options.ErrorMessage, languageCode);
			_emptyMessage = Localization.CreateLazyLocalizedString(options.EmptyMessage, languageCode);
		}

		public string CssClass { get; set; }

		public string GridCssClass { get; set; }

		public string LoadingMessage { get { return _loadingMessage.Value; } }

		public string ErrorMessage { get { return _errorMessage.Value; } }

		public string AccessDeniedMessage { get { return _accessDeniedMessage.Value; } }

		public string EmptyMessage { get { return _emptyMessage.Value; } }
	}
}
