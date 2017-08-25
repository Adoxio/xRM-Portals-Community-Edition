/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	internal class SalesAttachment : ISalesAttachment
	{
		public SalesAttachment(Entity salesLiteratureItem, EntityReference website)
		{
			if (salesLiteratureItem == null) throw new ArgumentNullException("salesLiteratureItem");
			if (salesLiteratureItem.LogicalName != "salesliteratureitem") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), salesLiteratureItem.LogicalName), "salesLiteratureItem");

			AbstractText = FormatAbstract(salesLiteratureItem.GetAttributeValue<string>("abstract"));
			AttachedDocumentURL = FormatAbstract(salesLiteratureItem.GetAttributeValue<string>("attacheddocumenturl"));
			AuthorName = salesLiteratureItem.GetAttributeValue<string>("authorname");
			ContentType = salesLiteratureItem.GetAttributeValue<string>("mimetype");
			FileName = salesLiteratureItem.GetAttributeValue<string>("filename");
			var filesize = salesLiteratureItem.GetAttributeValue<int?>("filesize").GetValueOrDefault();
			FileSize = new FileSize(Convert.ToUInt64(filesize < 0 ? 0 : filesize));
			HasFile = !string.IsNullOrEmpty(FileName);
			ID = salesLiteratureItem.Id;
			Keywords = salesLiteratureItem.GetAttributeValue<string>("keywords");
			Title = salesLiteratureItem.GetAttributeValue<string>("title");
			URL = salesLiteratureItem.GetFileAttachmentUrl(website);
		}

		public string AbstractText { get; private set; }

		public string AttachedDocumentURL { get; private set; }

		public string AuthorName { get; private set; }

		public string ContentType { get; private set; }

		public string FileName { get; private set; }

		public FileSize FileSize { get; private set; }

		public bool HasFile { get; private set; }

		public Guid ID { get; private set; }

		public string Keywords { get; private set; }

		public string Title { get; private set; }

		public string URL { get; private set; }

		private static string FormatAbstract(string abstractText)
		{
			return abstractText == null ? null : abstractText.TrimStart();
		}
	}
}
