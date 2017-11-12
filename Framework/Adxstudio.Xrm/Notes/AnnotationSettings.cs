/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Notes
{
	public class AnnotationSettings : IAnnotationSettings
	{
		private string _acceptMimeTypes;
		private string _acceptExtensionTypes;
		// Max possible CRM size
		private const ulong _defaultMaxFileSize = 32 << 10 << 10; // MB<<kB<<B

		private readonly string _defaultRestrictFileTypesErrorMessage = ResourceManager.GetString("Restrict_FileTypes_ErrorMessage");
		private readonly string _defaultMaxFileSizeErrorMessage = ResourceManager.GetString("Max_FileSize_ErrorMessage");
		private readonly string _defaultRestrictedFileExtensionsErrorMessage = ResourceManager.GetString("Restricted_FileExtensions_ErrorMessage");
		private readonly string _invalidFileExtenstionErrorMessage = ResourceManager.GetString("Invalid_FileExtenstion_ErrorMessage");

		public AnnotationSettings()
		{
			RespectPermissions = false;
			StorageLocation = StorageLocation.CrmDocument;
			AcceptMimeTypes = "*/*";
			AcceptExtensionTypes = ".*";
			RestrictMimeTypes = false;
			RestrictMimeTypesErrorMessage = _defaultRestrictFileTypesErrorMessage;
			MaxFileSize = null;
			MaxFileSizeErrorMessage = _defaultMaxFileSizeErrorMessage;
			RestrictedFileExtensions = string.Empty;
			RestrictedFileExtensionsErrorMessage = _defaultRestrictedFileExtensionsErrorMessage;
			InvalidFileExtenstionErrorMessage = _invalidFileExtenstionErrorMessage;
			IsPortalComment = false;
		}

		public AnnotationSettings(OrganizationServiceContext context, 
			bool respectPermissions = false,
			StorageLocation storageLocation = StorageLocation.CrmDocument, 
			string acceptMimeTypes = "*/*",
			bool restrictMimeTypes = false,
			string restrictMimeTypesErrorMessage = "",
			ulong? maxFileSize = null,
			string maxFileSizeErrorMessage = "",
			string acceptExtensionTypes = "",
			string restrictedFileExtensions = "",
			string restrictedFileExtensionsErrorMessage = "",
			string invalidFileExtenstionErrorMessage = "",
			bool isPortalComment = false)
		{
			IsPortalComment = isPortalComment;
			StorageLocation = storageLocation;
			RespectPermissions = respectPermissions;
			AcceptMimeTypes = acceptMimeTypes;
			AcceptExtensionTypes = acceptExtensionTypes;
			RestrictMimeTypes = restrictMimeTypes;
			RestrictMimeTypesErrorMessage = string.IsNullOrEmpty(restrictMimeTypesErrorMessage)
				? _defaultRestrictFileTypesErrorMessage
				: restrictMimeTypesErrorMessage;
			MaxFileSizeErrorMessage = string.IsNullOrEmpty(maxFileSizeErrorMessage)
				? _defaultMaxFileSizeErrorMessage
				: maxFileSizeErrorMessage;
			RestrictedFileExtensionsErrorMessage = string.IsNullOrEmpty(restrictedFileExtensionsErrorMessage)
				? _defaultRestrictedFileExtensionsErrorMessage
				: restrictedFileExtensionsErrorMessage;
			switch (StorageLocation)
			{
			case StorageLocation.CrmDocument:
				var org = context.GetOrganizationEntity(new[] { "maxuploadfilesize", "blockedattachments" });
				
				var orgMaxFileSize = Convert.ToUInt64(org.GetAttributeValue<int>("maxuploadfilesize"));
				MaxFileSize = Math.Min((maxFileSize.HasValue ? maxFileSize.Value : _defaultMaxFileSize), orgMaxFileSize);

				var orgRestrictedTypes = org.GetAttributeValue<string>("blockedattachments")
					.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
				var customRestrictedTypes = restrictedFileExtensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
				RestrictedFileExtensions = string.Join(";", orgRestrictedTypes.Union(customRestrictedTypes));
				break;
			default:
				MaxFileSize = maxFileSize;
				RestrictedFileExtensions = restrictedFileExtensions;
				break;
			}
			InvalidFileExtenstionErrorMessage = string.IsNullOrEmpty(invalidFileExtenstionErrorMessage)
				? _invalidFileExtenstionErrorMessage
				: invalidFileExtenstionErrorMessage;
		}

		public bool RespectPermissions { get; set; }
		public StorageLocation StorageLocation { get; set; }
		public string AcceptMimeTypes
		{
			get 
			{ 
				if (string.IsNullOrEmpty(_acceptMimeTypes))
				{
					return (IsPortalComment) ? string.Empty : "*/*";
				}
				else
				{
					return _acceptMimeTypes;
				}

			}
			set { _acceptMimeTypes = value; }
		}

		public string AcceptExtensionTypes
		{
			get { return string.IsNullOrEmpty(_acceptExtensionTypes) ? string.Empty : _acceptExtensionTypes; }
			set { _acceptExtensionTypes = value; }
		}

		public bool RestrictMimeTypes { get; set; }
		public string RestrictMimeTypesErrorMessage { get; set; }
		public ulong? MaxFileSize { get; set; }
		public string MaxFileSizeErrorMessage { get; set; }
		public string RestrictedFileExtensions { get; set; }
		public string RestrictedFileExtensionsErrorMessage { get; set; }
		public string InvalidFileExtenstionErrorMessage { get; set; }
		public bool IsPortalComment { get; set; }

	}
}
