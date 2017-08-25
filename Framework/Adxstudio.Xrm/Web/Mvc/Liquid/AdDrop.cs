/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class AdDrop : EntityDrop
	{
		private readonly Lazy<string> _adUrl;

		public AdDrop(IPortalLiquidContext portalLiquidContext, IAd ad)
			: base(portalLiquidContext, ad.Entity)
		{
			Ad = ad;
			Image = new AdImageDrop(Ad);

			_adUrl = new Lazy<string>(GetAdUrl, LazyThreadSafetyMode.None);
		}

		protected IAd Ad { get; private set; }

		public string Copy
		{
			get { return Ad.Copy; }
		}

		public AdImageDrop Image { get; private set; }

		public string Name
		{
			get { return Ad.Name; }
		}

		public bool OpenInNewWindow
		{
			get { return Ad.OpenInNewWindow; }
		}

		public string RedirectUrl
		{
			get { return Ad.RedirectUrl; }
		}

		public string Title
		{
			get { return Ad.Title; }
		}

		public string AdUrl
		{
			get { return _adUrl.Value; }
		}

		private string GetAdUrl()
		{
			return UrlHelper.RouteUrl(AdDataAdapter.AdRoute, new
			{
				id = Id,
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id
			});
		}
	}
}
