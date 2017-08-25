/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public class OpenCommandResponse : CommandResponse
	{
		[DataMember]
		public DirectoryContent cwd { get; set; }

		[DataMember]
		public DirectoryContent[] cdc { get; set; }

		[DataMember]
		public DirectoryTreeNode tree { get; set; }

		[DataMember]
		public bool tmb { get; set; }

		[DataMember]
		public string[] disabled { get; set; }

		[DataMember(Name = "params")]
		public InitializationParameters parameters { get; set; }
	}
}
