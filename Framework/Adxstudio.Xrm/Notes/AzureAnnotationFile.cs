/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Adxstudio.Xrm.Notes
{
	internal class AzureAnnotationFile : AnnotationFile, IAzureAnnotationFile
	{
		public CloudBlockBlob BlockBlob { get; set; }
		
		public AzureAnnotationFile()
		{
		}

		public AzureAnnotationFile(HttpPostedFileBase file) : base(file)
		{
			BlockBlob = null;
		}

		public AzureAnnotationFile(string fileName, string contentType, byte[] fileContent)
			: base(fileName, contentType, fileContent)
		{
			BlockBlob = null;
		}
	}
}
