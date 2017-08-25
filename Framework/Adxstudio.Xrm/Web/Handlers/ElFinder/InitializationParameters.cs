/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public class InitializationParameters
	{
		[DataMember]
		public string url { get; set; }

		[DataMember]
		public bool dotFiles { get; set; }

		[DataMember]
		public string uplMaxSize { get; set; }

		[DataMember]
		public string[] extract { get; set; }

		[DataMember]
		public string[] archives { get; set; }
	}
}
