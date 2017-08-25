/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Adxstudio.Xrm.Web.UI.HtmlControls
{
	internal class SelfClosingHtmlGenericControl : HtmlGenericControl
	{
		public SelfClosingHtmlGenericControl(string tag) : base(tag) { }

		protected override void Render(HtmlTextWriter writer)
		{
			if (Controls.Count > 0)
			{
				base.Render(writer);
			}
			else
			{
				writer.Write(HtmlTextWriter.TagLeftChar + TagName);

				Attributes.Render(writer);

				writer.Write(HtmlTextWriter.SelfClosingTagEnd);
			}
		}
	}
}
