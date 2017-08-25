/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Search
{
	public class CrmEntitySearchResult : ICrmEntitySearchResult
	{
		public CrmEntitySearchResult(Entity entity, float score, int resultNumber, string title, Uri url)
			: this(entity, score, resultNumber, title, url, new Dictionary<string, string>()) { }

		public CrmEntitySearchResult(Entity entity, float score, int resultNumber, string title, Uri url, IDictionary<string, string> extendedAttributes)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (title == null)
			{
				throw new ArgumentNullException("title");
			}

			if (resultNumber < 1)
			{
				throw new ArgumentException("Must be greater than 1.", "resultNumber");
			}

			if (extendedAttributes == null)
			{
				throw new ArgumentNullException("extendedAttributes");
			}

			Entity = entity;
			Score = score;
			Title = title;
			Url = url;
			ResultNumber = resultNumber;
			ExtendedAttributes = new Dictionary<string, string>(extendedAttributes);
		}

		public Entity Entity { get; private set; }

		public Guid EntityID
		{
			get { return Entity.Id; }
		}

		public string EntityLogicalName
		{
			get { return Entity.LogicalName; }
		}

		public Dictionary<string, string> ExtendedAttributes { get; private set; }

		public string Fragment { get; set; }

		public int ResultNumber { get; private set; }

		public float Score { get; private set; }

		public string Title { get; private set; }

		public Uri Url { get; private set; }
	}
}
