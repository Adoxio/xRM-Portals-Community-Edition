/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI.HtmlControls;
using HtmlAgilityPack;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using System.Linq;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Web Resource of type HTML.
	/// </summary>
	public class HtmlWebResourceControlTemplate : WebResourceCellTemplate
	{
		/// <summary>
		/// HtmlWebResourceControlTemplate class initialization.
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="contextName"></param>
		/// <param name="renderWebResourcesInline"></param>
		public HtmlWebResourceControlTemplate(FormXmlCellMetadata metadata, string contextName, bool? renderWebResourcesInline) : base(metadata)
		{
			ContextName = contextName;

			RenderWebResourcesInline = renderWebResourcesInline;
		}

		protected string ContextName { get; set; }

		protected bool? RenderWebResourcesInline { get; set; }

		protected override void InstantiateControlIn(HtmlControl container)
		{
			if (RenderWebResourcesInline == true)
			{
				var context = CrmConfigurationManager.CreateContext(ContextName);

				var webResource =
					context.CreateQuery("webresource").FirstOrDefault(
						wr => wr.GetAttributeValue<string>("name") == Metadata.WebResourceUrl);

				if (webResource == null || string.IsNullOrWhiteSpace(webResource.GetAttributeValue<string>("content")))
				{
					var placeholder = new HtmlGenericControl("literal") { ID = Metadata.ControlID, Visible = Enabled };

					container.Controls.Add(placeholder);

					return;
				}

				var htmlContent = new HtmlDocument();

				var webResourceContent = DecodeFrom64(webResource.GetAttributeValue<string>("content"));

				htmlContent.LoadHtml(webResourceContent);

				var body = htmlContent.DocumentNode.SelectSingleNode("//body");

				var literal = new HtmlGenericControl("literal") { ID = Metadata.ControlID, Visible = Enabled };

				if (body != null)
				{
					literal.InnerHtml = body.InnerHtml;

					container.Controls.Add(literal);
				}
			}
			else
			{
				var iframe = new HtmlGenericControl("iframe") { ID = Metadata.ControlID, Visible = Enabled };

				iframe.Attributes["src"] = WebResourceRouteFormat.FormatWith(Metadata.WebResourceUrl);
				iframe.Attributes["scrolling"] = Metadata.WebResourceScrolling;
				iframe.Attributes["frameborder"] = Metadata.WebResourceBorder ? "1" : "0";
				iframe.Attributes["height"] = "100%";
				iframe.Attributes["width"] = "100%";

				container.Controls.Add(iframe);
			}
		}

		/// <summary>
		/// Decode a base 64 encoded string.
		/// </summary>
		/// <param name="encodedData"></param>
		/// <returns></returns>
		public static string DecodeFrom64(string encodedData)
		{
			byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);

			string returnValue = System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);

			return returnValue;
		}
	}
}
