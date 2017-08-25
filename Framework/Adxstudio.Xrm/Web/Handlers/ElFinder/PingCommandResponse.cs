/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public class PingCommandResponse : CommandResponse
	{
		public PingCommandResponse()
		{
			CloseConnection = true;
		}
	}
}
