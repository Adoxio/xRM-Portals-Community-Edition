/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public class ThumbnailResponse : CommandResponse
	{
		[DataMember]
		public string current { get; set; }

		[DataMember]
		public object images { get; set; }

		[DataMember]
		public bool tmb { get; set; }
	}
}
