/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Notes
{
	public interface ICrmAnnotationFile : IAnnotationFile
	{
		byte[] Document { get; set; }
		void SetDocument(Func<byte[]> func);
	}
}
