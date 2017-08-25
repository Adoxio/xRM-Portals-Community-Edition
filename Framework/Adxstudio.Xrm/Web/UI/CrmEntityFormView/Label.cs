/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	internal class Label : System.Web.UI.WebControls.Label
	{
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Label;
			}
		}
	}
}
