/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Notes
{
	public interface IAnnotationResult
	{
		IAnnotation Annotation { get; }
		bool PermissionsExist { get; }
		bool PermissionGranted { get; }
	}
}
