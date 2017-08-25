/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Ideas
{
	internal class IdeaCommentFactory
	{
		private readonly OrganizationServiceContext _serviceContext;

		public IdeaCommentFactory(OrganizationServiceContext serviceContext)
		{
			serviceContext.ThrowOnNull("serviceContext");

			_serviceContext = serviceContext;
		}

		public IEnumerable<IComment> Create(IEnumerable<Entity> ideaComments)
		{
			var comments = ideaComments.ToArray();

			var extendedDatas = _serviceContext.FetchIdeaCommentExtendedData(comments.Select(e => e.Id));

			return comments.Select(e =>
			{
				Tuple<string, string> extendedDataValue;
				var extendedData = extendedDatas.TryGetValue(e.Id, out extendedDataValue)
					? extendedDataValue
					: new Tuple<string, string>(null, null);

				return new IdeaComment(e, extendedData.Item1, extendedData.Item2);
			}).ToArray();
		}
	}
}
