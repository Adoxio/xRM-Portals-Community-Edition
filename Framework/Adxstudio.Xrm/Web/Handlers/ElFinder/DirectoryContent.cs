/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public class DirectoryContent
	{
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public string hash { get; set; }

		[DataMember]
		public string rel { get; set; }

		[DataMember]
		public string url { get; set; }

		[DataMember]
		public string date { get; set; }

		[DataMember]
		public string mime { get; set; }

		[DataMember]
		public int size { get; set; }

		[DataMember]
		public bool read { get; set; }

		[DataMember]
		public bool write { get; set; }

		[DataMember]
		public bool rm { get; set; }

		[DataMember]
		public string link { get; set; }

		[DataMember]
		public string linkTo { get; set; }

		[DataMember]
		public string parent { get; set; }

		[DataMember]
		public bool resize { get; set; }

		[DataMember]
		public string dim { get; set; }

		[DataMember]
		public string tmb { get; set; }
	}
}
