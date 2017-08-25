/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Represents the HTML link element.
	/// </summary>
	public class HtmlLink : System.Web.UI.HtmlControls.HtmlLink
	{
		/// <summary>
		/// Gets or sets the URL target which may be a relative virtual path.
		/// </summary>
		public override string Href
		{
			get { return base.Href; }
			set { base.Href = ResolveUrl(value); }
		}
	}
}
