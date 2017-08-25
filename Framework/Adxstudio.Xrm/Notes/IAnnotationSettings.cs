/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Notes
{
	public interface IAnnotationSettings
	{
		bool RespectPermissions { get; set; }
		StorageLocation StorageLocation { get; set; }
		string AcceptMimeTypes { get; set; }
		string AcceptExtensionTypes { get; set; }
		bool RestrictMimeTypes { get; set; }
		string RestrictMimeTypesErrorMessage { get; set; }
		ulong? MaxFileSize { get; set; }
		string MaxFileSizeErrorMessage { get; set; }
		string RestrictedFileExtensions { get; set; }
		string RestrictedFileExtensionsErrorMessage { get; set; }
		string InvalidFileExtenstionErrorMessage { get; set; }
		bool IsPortalComment { get; set; }
	}

	public enum StorageLocation
	{
		CrmDocument = 756150000,
		AzureBlobStorage = 756150001
	}
}
