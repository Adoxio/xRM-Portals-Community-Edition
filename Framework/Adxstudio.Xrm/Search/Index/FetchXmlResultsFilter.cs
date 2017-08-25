/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Index
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;

	/// <summary>
	/// Fetch Xml results when 1:M of M:M are retrieved 
	/// </summary>
	public class FetchXmlResultsFilter
	{
		/// <summary>
		/// The last result on this page, which might be the same article as the first result on the next page.
		/// </summary>
		private XElement lastResultOnPage;

		/// <summary>
		/// Receives entire Fetch Xml and aggregates duplicate results caused by having N:N or 1:N relationships.
		/// </summary>
		/// <param name="fetchXml">The fetch XML.</param>
		/// <param name="primaryKeyFieldName">Entity Id field name</param>
		/// <param name="fields">List of fields</param>
		/// <returns>Aggregated results on Fetch Xml</returns>
		public string Aggregate(string fetchXml, string primaryKeyFieldName, params string[] fields)
		{
			var fetchXmlParsed = XDocument.Parse(fetchXml);
			var inputResults = fetchXmlParsed.Descendants("result").ToList();

			if (inputResults.Count == 0)
			{
				return fetchXml;
			}

			var aggregatedResults = new Dictionary<string, XElement>();
			var parsedResultSet = new FetchXmlResultSet(fetchXml);

			bool isFirstPage = this.lastResultOnPage == null;
			bool isLastPage = !parsedResultSet.MoreRecords;

			//// Check if last result of last page and first result of this page are the same article.
			//// If not, we need to add the aggregated result from last page during this round.
			//// If so, the past CAL/product ids should still be stored and we'll just add to 
			if (!isFirstPage)
			{
				var firstId = inputResults.First().Descendants(primaryKeyFieldName).FirstOrDefault().Value;
				var previousPageLastId = this.lastResultOnPage.Descendants(primaryKeyFieldName).FirstOrDefault().Value;
				if (firstId != previousPageLastId)
				{
					aggregatedResults[previousPageLastId] = this.lastResultOnPage;
				}
			}
			var lastId = inputResults.Descendants(primaryKeyFieldName).FirstOrDefault().Value;

			var collectionOfFields = fields.Select(fieldName => new RelatedField(fieldName)).ToList();

			//// Iterating through fetchXml retrieving multiple related fields
			foreach (var resultNode in inputResults)
			{
				var primaryKeyFieldNode = resultNode.Descendants(primaryKeyFieldName).FirstOrDefault();
				if (primaryKeyFieldNode == null) { return fetchXml; }

				////Retrieving fields
				collectionOfFields.ForEach(field => this.GetRelatedFields(resultNode, field, primaryKeyFieldNode.Value));
				////Removing duplicate nodes
				aggregatedResults[primaryKeyFieldNode.Value] = resultNode;
			}

			var node = inputResults.FirstOrDefault();
			if (node == null)
			{
				return fetchXml;
			}
			var parentNode = node.Parent;
			if (parentNode == null)
			{
				return fetchXml;
			}

			fetchXmlParsed.Descendants("result").Remove();

			//// Inserting retrieved above related fields and deduplicated results.
			collectionOfFields.ForEach(field => this.InsertRelatedFields(aggregatedResults, field));

			//// Remove and store the last aggregated result, as this might be the same article as the first result on the 
			//// next page.
			this.lastResultOnPage = aggregatedResults[lastId];
			if (!isLastPage)
			{
				aggregatedResults.Remove(lastId);
			}

			fetchXmlParsed.Element(parentNode.Name).Add(aggregatedResults.Values);
			return fetchXmlParsed.ToString();
		}

		/// <summary>
		/// Gets related fields
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="relatedField">The related fields.</param>
		/// <param name="entityId">Entity Id.</param>
		private void GetRelatedFields(XElement node, RelatedField relatedField, string entityId)
		{
			var relation = node.Descendants(relatedField.FieldName).FirstOrDefault();

			if (relation == null)
			{
				return;
			}

			var relationId = relation.Value;

			if (relatedField.ListOfFields.ContainsKey(entityId))
			{
				if (!relatedField.ListOfFields[entityId].Contains(relationId))
				{
					relatedField.ListOfFields[entityId] += ',' + relationId;
				}
			}
			else
			{
				relatedField.ListOfFields.Add(entityId, relationId);
			}
		}

		/// <summary>
		/// Inserts related fields to results.
		/// </summary>
		/// <param name="listOfNodes">The k a list.</param>
		/// <param name="relatedField">Related Fields</param>
		private void InsertRelatedFields(Dictionary<string, XElement> listOfNodes, RelatedField relatedField)
		{
			foreach (var relationship in relatedField.ListOfFields)
			{
				if (relationship.Value.Split(',').Length <= 1)
				{
					continue;
				}

				var relatedEntitiesIds = relationship.Value.Split(',');
				var node = listOfNodes[relationship.Key];

				var field = node.Descendants(relatedField.FieldName).FirstOrDefault();
				if (field != null)
				{
					field.Remove();
				}

				foreach (var id in relatedEntitiesIds)
				{
					node.Add(new XElement(relatedField.FieldName, id));
				}
				listOfNodes[relationship.Key] = node;
			}
		}

		/// <summary>
		/// Class holds name of field and list of fields
		/// </summary>
		private class RelatedField
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="RelatedField"/> class. 
			/// </summary>
			/// <param name="fieldName">Field name</param>
			public RelatedField(string fieldName)
			{
				this.FieldName = fieldName;
				this.ListOfFields = new Dictionary<string, string>();
			}

			/// <summary>
			/// Field name
			/// </summary>
			public string FieldName { get; private set; }

			/// <summary>
			/// List of fields
			/// </summary>
			public Dictionary<string, string> ListOfFields { get; set; }
		}
	}
}
