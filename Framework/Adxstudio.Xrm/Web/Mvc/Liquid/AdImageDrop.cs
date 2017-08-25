/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class AdImageDrop : Drop
	{
		public AdImageDrop(IAd ad)
		{
			if (ad == null) throw new ArgumentNullException("ad");

			Ad = ad;
		}

		public string AlternateText
		{
			get { return Ad.ImageAlternateText; }
		}

		public int? Height
		{
			get { return Ad.ImageHeight; }
		}

		public string Url
		{
			get { return Ad.ImageUrl; }
		}

		public int? Width
		{
			get { return Ad.ImageWidth; }
		}

		protected IAd Ad { get; private set; }
	}
}
