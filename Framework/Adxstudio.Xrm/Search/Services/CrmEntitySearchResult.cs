/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Search.Services
{
	[DataContract]
	public class CrmEntitySearchResult : ICrmEntitySearchResult
	{
		public CrmEntitySearchResult(string entityLogicalName, Guid entityID, string title, Uri url, string fragment, int resultNumber, float score)
			: this(entityLogicalName, entityID, title, url, fragment, resultNumber, score, new Dictionary<string, string>()) { }

		public CrmEntitySearchResult(string entityLogicalName, Guid entityID, string title, Uri url, string fragment, int resultNumber, float score, IDictionary<string, string> extendedAttributes)
		{
			if (extendedAttributes == null)
			{
				throw new ArgumentNullException("extendedAttributes");
			}

			EntityLogicalName = entityLogicalName;
			EntityID = entityID;
			Title = title;
			Url = url;
			Fragment = fragment;
			ResultNumber = resultNumber;
			Score = score;
			ExtendedAttributes = new Dictionary<string, string>(extendedAttributes);
		}

		public Entity Entity
		{
			get { throw new NotSupportedException("This property is not supported by this type. (Full entities are not loaded by search results returned through services.)"); }
		}

		[DataMember]
		public Guid EntityID { get; private set; }

		[DataMember]
		public string EntityLogicalName { get; private set; }

		[DataMember]
		public Dictionary<string, string> ExtendedAttributes { get; private set; }

		[DataMember]
		public string Fragment { get; set; }

		[DataMember]
		public int ResultNumber { get; private set; }

		[DataMember]
		public float Score { get; private set; }

		[DataMember]
		public string Title { get; private set; }

		[DataMember]
		public Uri Url { get; private set; }
	}
}
