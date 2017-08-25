/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.WindowsAzure.Storage.Blob;

namespace Adxstudio.Xrm.Notes
{
	public interface IAzureAnnotationFile : IAnnotationFile
	{
		CloudBlockBlob BlockBlob { get; set; }
	}
}
