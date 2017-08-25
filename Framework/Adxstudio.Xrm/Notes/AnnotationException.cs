/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Notes
{
	[Serializable]
	public class AnnotationException : Exception
	{
		public AnnotationException(string message) : base(message)
		{
		}
	}
}
