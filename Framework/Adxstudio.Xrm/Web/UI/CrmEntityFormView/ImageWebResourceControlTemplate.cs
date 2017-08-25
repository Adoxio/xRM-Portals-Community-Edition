/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI.HtmlControls;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Web Resource of type Image.
	/// </summary>
	public class ImageWebResourceControlTemplate : WebResourceCellTemplate
	{
		/// <summary>
		/// ImageWebResourceControlTemplate class initialization.
		/// </summary>
		/// <param name="metadata"></param>
		public ImageWebResourceControlTemplate(FormXmlCellMetadata metadata) : base(metadata) { }
		
		protected override void InstantiateControlIn(HtmlControl container)
		{
			var image = new HtmlImage
			{
				ID = Metadata.ControlID,
				Alt = Metadata.WebResourceAltText,
				Src = WebResourceRouteFormat.FormatWith(Metadata.WebResourceUrl),
				Visible = Enabled
			};

			image.Attributes["class"] = CssClass;
			container.Attributes.CssStyle["text-align"] = Metadata.WebResourceHorizontalAlignment;
			container.Attributes.CssStyle["vertical-align"] = Metadata.WebResourceVerticalAlignment;
			
			if (Metadata.WebResourceSizeType == "Specific")
			{
				image.Height = Metadata.WebResourceHeight.GetValueOrDefault();
				image.Width = Metadata.WebResourceWidth.GetValueOrDefault();
			}

			container.Controls.Add(image);
		}
	}
}
