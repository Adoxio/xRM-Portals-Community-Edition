/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Search.WindowsAzure
{
	[DataContract]
	public class EntitySearchResult
	{
		public EntitySearchResult(string entityLogicalName, Guid entityID, string title, string fragment, float score)
			: this(entityLogicalName, entityID, title, fragment, score, new Dictionary<string, string>()) { }

		public EntitySearchResult(string entityLogicalName, Guid entityID, string title, string fragment, float score, IDictionary<string, string> extendedAttributes)
		{
			if (extendedAttributes == null)
			{
				throw new ArgumentNullException("extendedAttributes");
			}

			EntityLogicalName = entityLogicalName;
			EntityID = entityID;
			Fragment = fragment;
			Score = score;
			Title = title;
		}

		[DataMember]
		public Guid EntityID { get; set; }

		[DataMember]
		public string EntityLogicalName { get; set; }

		[DataMember]
		public string Fragment { get; set; }

		[DataMember]
		public float Score { get; set; }

		[DataMember]
		public string Title { get; set; }
	}
}
