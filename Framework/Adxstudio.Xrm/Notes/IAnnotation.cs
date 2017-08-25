/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Notes
{
	public interface IAnnotation
	{
		Entity Entity { get; set; }
		Guid AnnotationId { get; set; }
		string Subject { get; set; }
		string NoteText { get; set; }
		EntityReference Regarding { get; set; }
		IAnnotationFile FileAttachment { get; set; }
		DateTime CreatedOn { get; set; }
		EntityReference Owner { get; set; }
	}
}
