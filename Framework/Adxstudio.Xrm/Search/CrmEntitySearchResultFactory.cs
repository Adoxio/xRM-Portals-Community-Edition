/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Lucene.Net.Documents;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Search
{
	public class CrmEntitySearchResultFactory : ICrmEntitySearchResultFactory
	{
		public CrmEntitySearchResultFactory(ICrmEntityIndex index, ICrmEntitySearchResultFragmentProvider fragmentProvider)
		{
			if (index == null)
			{
				throw new ArgumentNullException("index");
			}

			if (fragmentProvider == null)
			{
				throw new ArgumentNullException("fragmentProvider");
			}

			Index = index;
			FragmentProvider = fragmentProvider;
		}

		protected ICrmEntitySearchResultFragmentProvider FragmentProvider { get; private set; }

		protected ICrmEntityIndex Index { get; private set; }

		public virtual ICrmEntitySearchResult GetResult(Document document, float score, int number)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}

			var logicalNameField = document.GetField(Index.LogicalNameFieldName);

			if (logicalNameField == null || !logicalNameField.IsStored)
			{
				return null;
			}

			var logicalName = logicalNameField.StringValue;

			var primaryKeyLogicalNameField = document.GetField(Index.PrimaryKeyLogicalNameFieldName);

			if (primaryKeyLogicalNameField == null || !primaryKeyLogicalNameField.IsStored)
			{
				return null;
			}

			var primaryKeyPropertyName = primaryKeyLogicalNameField.StringValue;

			var context = Index.DataContext;

			var primaryKeyField = document.GetField(Index.PrimaryKeyFieldName);

			if (primaryKeyField == null || !primaryKeyField.IsStored)
			{
				return null;
			}

			var primaryKey = new Guid(primaryKeyField.StringValue);

			var titleField = document.GetField(Index.TitleFieldName);

			var title = string.Empty;

			if (logicalName != "annotation")
			{
				if (titleField == null || !titleField.IsStored)
				{
					return null;
				}
				title = titleField.StringValue;
			}

			var entity = context.CreateQuery(logicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>(primaryKeyPropertyName) == primaryKey);

			if (entity == null)
			{
				return null;
			}

			var url = GetUrl(context, document, score, number, entity);

			var result = new CrmEntitySearchResult(entity, score, number, title, url);

			if (!Validate(context, result))
			{
				return null;
			}

			result.Fragment = FragmentProvider.GetFragment(document);

			return result;
		}

		protected virtual Uri GetUrl(OrganizationServiceContext context, Document document, float score, int number, Entity entity)
		{
			return null;
		}

		protected virtual bool Validate(OrganizationServiceContext context, CrmEntitySearchResult result)
		{
			return true;
		}
	}
}
