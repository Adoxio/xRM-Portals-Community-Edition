/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Search.Handlers
{
	[DataContract]
	public class ExceptionData
	{
		public ExceptionData(Exception exception)
		{
			Full = exception.ToString();
			Message = exception.Message;
		}

		[DataMember]
		public string Full { get; private set; }

		[DataMember]
		public string Message { get; private set; }
	}
}
