/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Notes
{
	public interface IAnnotationFile
	{
		string FileName { get; set; }
		string MimeType { get; set; }
		FileSize FileSize { get; set; }
		Entity Annotation { get; set; }
		string Url { get; }
		
		Stream GetFileStream();
		void SetAnnotation(Func<Entity> func);
	}
}
