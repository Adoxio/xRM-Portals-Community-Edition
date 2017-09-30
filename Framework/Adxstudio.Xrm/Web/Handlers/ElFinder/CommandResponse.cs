/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public abstract class CommandResponse
	{
		/// <summary>
		/// Used by the "ping" command to signal that an empty response with header "Connection: close" should
		/// be returned by the HTTP handler.
		/// </summary>
		public bool CloseConnection { get; set; }

		[DataMember]
		public string error { get; set; }

		[DataMember]
		public string[] select { get; set; }

		// Should also support errorData (Object) and debug (Object), but need more info on their contents.

		public string ToJson()
		{
			using (var stream = new MemoryStream())
			{
				var serializer = new DataContractJsonSerializer(GetType());

				serializer.WriteObject(stream, this);

				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
