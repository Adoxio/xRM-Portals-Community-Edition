/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Index
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.ContentAccess;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Adxstudio.Xrm.Resources;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

	/// <summary>
	/// Fetch Xml Indexer
	/// </summary>
	/// <seealso cref="Adxstudio.Xrm.Search.Index.ICrmEntityIndexer" />
	public class FetchXmlIndexer : ICrmEntityIndexer
	{
		private readonly FetchXml _fetchXml;
		private readonly ICrmEntityIndex _index;
		private readonly string _titleAttributeLogicalName;
		private readonly FetchXmlLocaleConfig _localeConfig;

		public FetchXmlIndexer(ICrmEntityIndex index, XNode fetchXml, string titleAttributeLogicalName)
			: this(index, new FetchXml(fetchXml), titleAttributeLogicalName) { }

		public FetchXmlIndexer(ICrmEntityIndex index, string fetchXml, string titleAttributeLogicalName)
			: this(index, XDocument.Parse(fetchXml), titleAttributeLogicalName) { }

		internal FetchXmlIndexer(ICrmEntityIndex index, FetchXml fetchXml, string titleAttributeLogicalName)
		{
			if (index == null)
			{
				throw new ArgumentNullException("index");
			}

			if (fetchXml == null)
			{
				throw new ArgumentNullException("fetchXml");
			}

			if (titleAttributeLogicalName == null)
			{
				throw new ArgumentNullException("titleAttributeLogicalName");
			}

			_index = index;
			_fetchXml = fetchXml;
			_titleAttributeLogicalName = titleAttributeLogicalName;

			if (!this._fetchXml.ContainsAttribute("modifiedon") && this._fetchXml.LogicalName != "annotation")
			{
				this._fetchXml.AddAttribute("modifiedon");
			}

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
			{
				if (this._fetchXml.LogicalName == "adx_blog")
				{
					if (!this._fetchXml.ContainsAttribute("adx_parentpageid"))
					{
						this._fetchXml.AddAttribute("adx_parentpageid");
					}
				}

				if (this._fetchXml.LogicalName == "adx_blogpost")
				{
					if (!this._fetchXml.ContainsAttribute("adx_blogid"))
					{
						this._fetchXml.AddAttribute("adx_blogid");
					}

					// Add the published attribute as we need it for indexing for CMS
					if (!this._fetchXml.ContainsAttribute("adx_published"))
					{
						this._fetchXml.AddAttribute("adx_published");
					}

					if (!this._fetchXml.ContainsLinkEntity("adx_blog_blogpost"))
					{
						this._fetchXml.AddLinkEntity("adx_blog", "adx_blogid", "adx_blogid", "adx_blog_blogpost", "inner");
					}
					this._fetchXml.AddLinkEntityAttribute("adx_blog_blogpost", "adx_parentpageid");
					this._fetchXml.AddLinkEntityAttribute("adx_blog_blogpost", "adx_partialurl");

					if (!this._fetchXml.ContainsAttribute("adx_partialurl"))
					{
						this._fetchXml.AddAttribute("adx_partialurl");
					}
				}

				if (this._fetchXml.LogicalName == "adx_idea")
				{
					if (!this._fetchXml.ContainsAttribute("adx_ideaforumid"))
					{
						this._fetchXml.AddAttribute("adx_ideaforumid");
					}

					// Add the approved flag as this is needed for indexing CMS
					this._fetchXml.AddConditionalStatement("and", "adx_approved", "eq", "true");

					if (!this._fetchXml.ContainsLinkEntity("adx_idea_ideaforum"))
					{
						this._fetchXml.AddLinkEntity("adx_ideaforum", "adx_ideaforumid", "adx_ideaforumid", "adx_idea_ideaforum", "inner");
					}
					this._fetchXml.AddLinkEntityAttribute("adx_idea_ideaforum", "adx_partialurl");
					this._fetchXml.AddConditionalStatement("and", "adx_partialurl", "not-null", null, "adx_idea_ideaforum");

					this._fetchXml.AddConditionalStatement("and", "adx_partialurl", "not-null");
					this._fetchXml.AddAttribute("adx_partialurl");
				}

				if (this._fetchXml.LogicalName == "adx_communityforumthread")
				{
					if (!this._fetchXml.ContainsAttribute("adx_forumid"))
					{
						this._fetchXml.AddAttribute("adx_forumid");
					}
				}

				if (this._fetchXml.LogicalName == "adx_communityforumpost")
				{
					if (!this._fetchXml.ContainsLinkEntity("adx_communityforumpost_communityforumthread"))
					{
						this._fetchXml.AddLinkEntity("adx_communityforumthread", "adx_communityforumthreadid", "adx_forumthreadid", "adx_communityforumpost_communityforumthread", "inner");
					}
					this._fetchXml.AddLinkEntityAttribute("adx_communityforumpost_communityforumthread", "adx_forumid");
				}

				if (this._fetchXml.LogicalName == "adx_webfile")
				{
					if (!this._fetchXml.ContainsAttribute("adx_parentpageid"))
					{
						this._fetchXml.AddAttribute("adx_parentpageid");
					}
					if (!this._fetchXml.ContainsAttribute("adx_partialurl"))
					{
						this._fetchXml.AddAttribute("adx_partialurl");
					}
				}

				if (this._fetchXml.LogicalName == "incident")
				{
					// It is marked as Resolved (1)
					this._fetchXml.AddConditionalStatement("and", "statecode", "eq", "1");
					this._fetchXml.AddConditionalStatement("and", "adx_publishtoweb", "eq", "1");
				}

				// CMS filtering for KnowledgeArticles if they don't have these rules then don't add to index.
				if (this._fetchXml.LogicalName == "knowledgearticle")
				{
					// make sure statecode is published = 3
					this._fetchXml.AddConditionalStatement("and", "statecode", "eq", "3");
					this._fetchXml.AddConditionalStatement("and", "isrootarticle", "eq", "false");
					this._fetchXml.AddConditionalStatement("and", "isinternal", "eq", "false");

					// Add this filter for url filtering
					this._fetchXml.AddConditionalStatement("and", "articlepublicnumber", "not-null");

					this.AddRelatedEntityFetch("connection", "connectionid", "record1id",
						"knowledgearticleid", "record2id", "product", "productid", "record2id", "productid");

					if (this._index.DataContext.AssertEntityExists("adx_contentaccesslevel"))
					{
						this.AddRelatedEntityFetch("adx_knowledgearticlecontentaccesslevel",
							"adx_knowledgearticlecontentaccesslevelid", "knowledgearticleid", "knowledgearticleid", "adx_contentaccesslevelid",
							"adx_contentaccesslevel", "adx_contentaccesslevelid", "adx_contentaccesslevelid", "adx_contentaccesslevelid");
					}
				}
			}

			// Add the language fields since the related fields cannot be included in a view using the savedquery editor
			if (_fetchXml.LogicalName == "knowledgearticle")
			{
				_fetchXml.AddLinkEntity("languagelocale", "languagelocaleid", "languagelocaleid", "language_localeid", "outer");
				_fetchXml.AddLinkEntityAttribute("language_localeid", "localeid");
				_fetchXml.AddLinkEntityAttribute("language_localeid", "code");
				_fetchXml.AddLinkEntityAttribute("language_localeid", "region");
				_fetchXml.AddLinkEntityAttribute("language_localeid", "name");
				_fetchXml.AddLinkEntityAttribute("language_localeid", "language");
				// This ensures we get knowledge article search result along with annotation in case knowledge article doesn't have keywords contained in annotation
				if (_index.DisplayNotes)
				{
					this.AddNotesLinkEntity(_index.NotesFilter);
				}

				_localeConfig = FetchXmlLocaleConfig.CreateKnowledgeArticleConfig();
			}
			else
			{
				_localeConfig = FetchXmlLocaleConfig.CreatePortalLanguageConfig();

			}

			if (_fetchXml.LogicalName == "annotation")
			{
				_fetchXml.AddConditionalStatement("and", "filename", "not-null");
				this.AddNotesFilter(_index.NotesFilter);
				this.AddRelatedKnowledgeArticleAndProductFetch();
			}
		}

		public bool Indexes(string entityLogicalName)
		{
			return string.Equals(_fetchXml.LogicalName, entityLogicalName, StringComparison.InvariantCultureIgnoreCase);
		}

		public IEnumerable<CrmEntityIndexDocument> GetDocuments()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var dataContext = _index.DataContext;
			var documentFactory = new FetchXmlIndexDocumentFactory(_index, _fetchXml, _titleAttributeLogicalName, _localeConfig);

			var currentPageFetchXml = _fetchXml;
			var knowledgeArticleFilter = new FetchXmlResultsFilter();

			while (true)
			{
				
				var request = new OrganizationRequest("ExecuteFetch");
				request.Parameters["FetchXml"] = currentPageFetchXml.ToString();

				var response = dataContext.Execute(request);

				if (response == null)
				{
					throw new InvalidOperationException("Did not receive valid response from ExecuteFetchRequest.");
				}

				var fetchXmlResponse = response.Results["FetchXmlResult"] as string;

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
				{
					if (this._fetchXml.LogicalName == "knowledgearticle")
					{
						fetchXmlResponse = knowledgeArticleFilter.Aggregate(fetchXmlResponse, "knowledgearticleid", "record2id.productid",
							"adx_contentaccesslevelid.adx_contentaccesslevelid", "record2id.connectionid", "annotation.filename",
							"annotation.notetext", "annotation.annotationid");
					}
					if (this._fetchXml.LogicalName == "annotation")
					{
						fetchXmlResponse = knowledgeArticleFilter.Aggregate(fetchXmlResponse, "annotationid", "product.productid");
					}
				}

				var resultSet = new FetchXmlResultSet(fetchXmlResponse);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("FetchXmlResult:LogicalName={0}, Count={1}", EntityNamePrivacy.GetEntityName(this._fetchXml.LogicalName), resultSet.Count()));

				foreach (var document in resultSet.Select(documentFactory.GetDocument))
				{
					yield return document;
				}

				if (resultSet.MoreRecords)
				{
					currentPageFetchXml = currentPageFetchXml.ForNextPage(resultSet.PagingCookie);
				}
				else
				{
					break;
				}
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");
		}

		private void AddRelatedEntityFetch(string intersectLinkEntityName, string intersectLinkEntityNamePrimaryAttribute,
			string intersectLinkEntityFrom, string intersectLinkEntityTo, string intersectLinkEntityAlias,
			string linkEntityName, string linkEntityFrom, string linkEntityTo, string attributeName)
		{
			var xml =
				@"<link-entity name='{0}' from='{1}' to='{2}' link-type='outer' alias='{3}'>
					<attribute name ='{4}' />
					<link-entity name ='{5}' from ='{6}' to ='{7}' link-type ='outer' >
						<attribute name ='{8}' />
						<filter >
							<condition attribute ='statuscode' operator='eq' value ='1' />
							<condition attribute ='statecode' operator='eq' value ='0' />
						</filter >
					</link-entity >
				</link-entity > ";

			this._fetchXml.AddLinkEntity(XElement.Parse(string.Format(xml, intersectLinkEntityName, intersectLinkEntityFrom,
				intersectLinkEntityTo, intersectLinkEntityAlias, intersectLinkEntityNamePrimaryAttribute, linkEntityName,
				 linkEntityFrom, linkEntityTo, attributeName)));
		}

		private void AddRelatedKnowledgeArticleAndProductFetch()
		{
			var fetchXml =
				@"<link-entity name='knowledgearticle' from='knowledgearticleid' to='objectid' alias='knowledgearticle' link-type='inner'>
							<attribute name ='rating' />
							<attribute name ='modifiedon' />
							<attribute name ='knowledgearticleid' />
								<link-entity name ='connection' from ='record2id' to='knowledgearticleid' link-type ='outer'>
									<link-entity name ='product' from ='productid' to='record1id' alias='product' link-type ='outer'>
									<attribute name ='productid'/>
										<filter >
											<condition attribute ='statecode' operator='eq' value ='0' />
											<condition attribute ='statuscode' operator='eq' value ='1' />
										</filter >
									</link-entity >
								</link-entity >
							</link-entity >";

			this._fetchXml.AddLinkEntity(XElement.Parse(fetchXml));
		}

		private void AddNotesFilter(string notePrefix)
		{
			var fetchXml = @"<filter>
							<condition attribute='notetext' operator='begins-with' value='{0}' />
							</filter > ";

			this._fetchXml.AddLinkEntity(XElement.Parse(string.Format(fetchXml, notePrefix)));
		}

		private void AddNotesLinkEntity(string notePrefix)
		{
			var fetchXml = @"<link-entity name='annotation' from='objectid' to='knowledgearticleid' link-type='outer' alias='annotation' >
								<attribute name='filename' />
								<attribute name='notetext' />
								<attribute name='annotationid' />
								<filter>
									<condition attribute='notetext' operator='begins-with' value='{0}' />
									<condition attribute='filename' operator='not-null' />
								</filter>
							</link-entity>";
			this._fetchXml.AddLinkEntity(XElement.Parse(string.Format(fetchXml, notePrefix)));
		}
	}
}
