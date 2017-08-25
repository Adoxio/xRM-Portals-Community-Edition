/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class WebLinkImageDrop : Drop
	{
		public WebLinkImageDrop(IWebLink webLink)
		{
			if (webLink == null) throw new ArgumentNullException("webLink");

			WebLink = webLink;
		}

		public string AlternateText
		{
			get { return WebLink.ImageAlternateText; }
		}

		public int? Height
		{
			get { return WebLink.ImageHeight; }
		}

		public string Url
		{
			get { return WebLink.ImageUrl; }
		}

		public int? Width
		{
			get { return WebLink.ImageWidth; }
		}

		protected IWebLink WebLink { get; private set; }
	}
}
