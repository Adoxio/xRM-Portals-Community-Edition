/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Search
{
	public class IndexNotFoundException : InvalidOperationException
	{
		public IndexNotFoundException() { }

		public IndexNotFoundException(string message) : base(message) { }

		public IndexNotFoundException(string message, Exception innerException) : base(message, innerException) { }
	}
}
