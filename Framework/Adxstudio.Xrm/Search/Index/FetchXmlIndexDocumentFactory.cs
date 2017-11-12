/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Index
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Resources;
	using Lucene.Net.Documents;
	using Microsoft.Xrm.Client;
	using Adxstudio.Xrm.Search.Facets;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Metadata;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.ContentAccess;
	using Adxstudio.Xrm.Core.Flighting;
	using Microsoft.Xrm.Portal.Configuration;

	internal class FetchXmlIndexDocumentFactory
	{
		private readonly OrganizationServiceContext _dataContext;
		private readonly FetchXml _fetchXml;
		private readonly ICrmEntityIndex _index;
		private readonly IDictionary<string, EntityMetadata> _metadataCache = new Dictionary<string, EntityMetadata>();
		private readonly string _titleAttributeLogicalName;
		private readonly FetchXmlLocaleConfig _localeConfig;
		private readonly IContentMapProvider _contentMapProvider;
		private readonly List<string> oOBUrlDefinedEntities = new List<string>()
											   {
												   "adx_blog", "adx_blogpost",
												   "adx_webpage",
												   "adx_webfile",
												   "adx_communityforum", "adx_communityforumthread", "adx_communityforumpost",
												   "adx_idea", "adx_ideaforum",
												   "incident",
											   };

		public FetchXmlIndexDocumentFactory(ICrmEntityIndex index, FetchXml fetchXml, string titleAttributeLogicalName, FetchXmlLocaleConfig localeConfig)
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

			if (localeConfig == null)
			{
				throw new ArgumentNullException("localeConfig");
			}

			_index = index;
			_contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider();
			_fetchXml = fetchXml;
			_titleAttributeLogicalName = titleAttributeLogicalName;
			_localeConfig = localeConfig;

			_dataContext = _index.DataContext;
		}

		/// <summary>
		/// Gets the document.
		/// </summary>
		/// <param name="fetchXmlResult">The fetch XML result.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">
		/// </exception>
		public CrmEntityIndexDocument GetDocument(FetchXmlResult fetchXmlResult)
		{
			var entityMetadata = _dataContext.GetEntityMetadata(_fetchXml.LogicalName, _metadataCache);

			var attributes = entityMetadata.Attributes.ToDictionary(a => a.LogicalName, a => a);

			var document = new Document();

			var primaryKey = Guid.Empty;

			var languageValueAdded = false;

			var lcid = 0;

			// Store the entity logical name and the logical name of the primary key attribute in the index document, for
			// easier later retrieval of the entity corresponding to this document.
			document.Add(
				new Field(_index.LogicalNameFieldName, entityMetadata.LogicalName, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(
				new Field(
					_index.PrimaryKeyLogicalNameFieldName,
					entityMetadata.PrimaryIdAttribute,
					Field.Store.YES,
					Field.Index.NOT_ANALYZED));
			try
			{
				var content = new ContentFieldBuilder();

				foreach (var fetchXmlField in fetchXmlResult)
				{
					// Treat the primary key field in a special way.
					if (fetchXmlField.Name == entityMetadata.PrimaryIdAttribute)
					{
						primaryKey = new Guid(fetchXmlField.Value);

						document.Add(new Field(_index.PrimaryKeyFieldName, primaryKey.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
						document.Add(new Field(fetchXmlField.Name, primaryKey.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

						// Adding webroles for the webpage to the index. 
						if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
						{
							if (entityMetadata.LogicalName == "adx_webpage")
							{
								ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "CMS is enabled. Adding roles for adx_webpage index");

								var ruleNames = CmsIndexHelper.GetWebPageWebRoles(this._contentMapProvider, primaryKey);
								this.AddWebRolesToDocument(document, ruleNames);
							}

							if (entityMetadata.LogicalName == "adx_ideaforum")
							{
								ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "CMS is enabled. Adding roles for adx_ideaforum index");

								var ruleNames = CmsIndexHelper.GetIdeaForumWebRoles(this._contentMapProvider, primaryKey);
								this.AddWebRolesToDocument(document, ruleNames);
							}

							if (entityMetadata.LogicalName == "adx_communityforum")
							{
								ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "CMS is enabled. Adding roles for adx_communityforum index");

								var ruleNames = CmsIndexHelper.GetForumsWebRoles(this._contentMapProvider, primaryKey);
								this.AddWebRolesToDocument(document, ruleNames);
							}
						}
						continue;
					}

					if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
					{
						if (fetchXmlField.Name == "adx_ideaforumid" && entityMetadata.LogicalName == "adx_idea")
						{
							ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "CMS is enabled. Adding roles for adx_idea index");

							var ruleNames = CmsIndexHelper.GetIdeaForumWebRoles(
							this._contentMapProvider,
							new Guid(fetchXmlField.Value));
							this.AddWebRolesToDocument(document, ruleNames);
						}

						// Based off the Parent Web Page get the webroles for each given entity
						if ((fetchXmlField.Name == "adx_parentpageid" && entityMetadata.LogicalName == "adx_blog")
							|| (fetchXmlField.Name == "adx_blog_blogpost.adx_parentpageid" && entityMetadata.LogicalName == "adx_blogpost")
							|| (fetchXmlField.Name == "adx_parentpageid" && entityMetadata.LogicalName == "adx_webfile"))
						{
							ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("CMS is enabled. Adding roles for {0} index", fetchXmlField.Name));

							var ruleNames = CmsIndexHelper.GetWebPageWebRoles(this._contentMapProvider, new Guid(fetchXmlField.Value));
							this.AddWebRolesToDocument(document, ruleNames);
						}

						if ((fetchXmlField.Name == "adx_forumid" && entityMetadata.LogicalName == "adx_communityforumthread")
						|| (fetchXmlField.Name == "adx_communityforumpost_communityforumthread.adx_forumid" && entityMetadata.LogicalName == "adx_communityforumpost"))
						{
							ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("CMS is enabled. Adding roles for {0} index", fetchXmlField.Name));

							var ruleNames = CmsIndexHelper.GetForumsWebRoles(
							this._contentMapProvider,
							new Guid(fetchXmlField.Value));
							this.AddWebRolesToDocument(document, ruleNames);
						}
						if (entityMetadata.LogicalName == "annotation" && fetchXmlField.Name == "knowledgearticle.knowledgearticleid")
						{
							var id = new Guid(fetchXmlField.Value);
							document.Add(new Field("annotation_knowledgearticleid", id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
						}
					}


					// Store the title of the result in a special field.
					if (fetchXmlField.Name == _titleAttributeLogicalName && entityMetadata.LogicalName != "annotation")
					{
						document.Add(new Field(_index.TitleFieldName, fetchXmlField.Value, Field.Store.YES, Field.Index.ANALYZED));
					}

					// Store the language locale code in a separate field.
					if (_localeConfig.IsLanguageCodeLogicalName(fetchXmlField.Name))
					{
						document.Add(
							new Field(
								_index.LanguageLocaleCodeFieldName,
								fetchXmlField.Value.ToLowerInvariant(),
								Field.Store.YES,
								Field.Index.NOT_ANALYZED));
						languageValueAdded = true;
					}

					// Store the language locale LCID in a separate field.
					if (_localeConfig.IsLCIDLogicalName(fetchXmlField.Name) && int.TryParse(fetchXmlField.Value, out lcid))
					{
						document.Add(
							new Field(_index.LanguageLocaleLCIDFieldName, fetchXmlField.Value, Field.Store.YES, Field.Index.NOT_ANALYZED));
					}

					// Skip metadata parsing for language fields
					if (_localeConfig.CanSkipMetadata(fetchXmlField.Name)) continue;

					FetchXmlLinkAttribute link;

					if (_fetchXml.TryGetLinkAttribute(fetchXmlField, out link))
					{
						var linkEntityMetadata = _dataContext.GetEntityMetadata(link.EntityLogicalName, _metadataCache);

						var linkAttributeMetadata = linkEntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == link.LogicalName);

						if (linkAttributeMetadata == null)
						{
							throw new InvalidOperationException("Unable to retrieve attribute metadata for FetchXML result field {0} for entity {1}.".FormatWith(link.LogicalName, linkEntityMetadata.LogicalName));
						}

						var fieldName = fetchXmlResult.Any(f => f.Name == link.LogicalName)
											? "{0}.{1}".FormatWith(linkEntityMetadata.LogicalName, linkAttributeMetadata.LogicalName)
											: link.LogicalName;

						//Renaming product identifier field to  "associated.product"
						if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching)
							&& (this._fetchXml.LogicalName == "knowledgearticle" && fieldName == "record2id" 
							|| this._fetchXml.LogicalName == "annotation" && fieldName == "productid"))
						{
							fieldName = FixedFacetsConfiguration.ProductFieldFacetName;
						}
						if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching)
						&& (this._fetchXml.LogicalName == "knowledgearticle" && (fieldName == "notetext" || fieldName == "filename")))
						{
							fieldName = "related_" + fieldName;
						}

						if (fieldName == "related_filename")
						{
							fetchXmlField.Value = Regex.Replace(fetchXmlField.Value, "[._,-]", " ");
						}
						if (fieldName == "related_notetext")
						{
							fetchXmlField.Value = fetchXmlField.Value.Substring(GetNotesFilterPrefix().Length);
						}

						AddDocumentFields(document, fieldName, fetchXmlField, linkAttributeMetadata, content);
					}
					else
					{
						AttributeMetadata attributeMetadata;

						if (!attributes.TryGetValue(fetchXmlField.Name, out attributeMetadata))
						{
							throw new InvalidOperationException(
								ResourceManager.GetString("Attribute_Metadata_Fetchxml_Retrieve_Exception")
									.FormatWith(fetchXmlField.Name, entityMetadata.LogicalName));
						}

						if (fetchXmlField.Name == "filename")
						{
							fetchXmlField.Value = Regex.Replace(fetchXmlField.Value, "[._,-]", " ");
						}
						if (fetchXmlField.Name == "notetext")
						{
							fetchXmlField.Value = fetchXmlField.Value.Substring(GetNotesFilterPrefix().Length);
						}

						AddDocumentFields(document, fetchXmlField.Name, fetchXmlField, attributeMetadata, content);
					}
				}

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
				{
					// Add the default value for entities that are not Knowledge articles for the Product filtering field. 
					if (entityMetadata.LogicalName != "knowledgearticle")
					{
						document.Add(
							new Field(
								FixedFacetsConfiguration.ProductFieldFacetName,
								this._index.ProductAccessNonKnowledgeArticleDefaultValue,
								Field.Store.NO,
								Field.Index.NOT_ANALYZED));
						document.Add(
							new Field(FixedFacetsConfiguration.ContentAccessLevel,
							"public",
							Field.Store.NO,
							Field.Index.NOT_ANALYZED));
					}
					else
					{
						// If there aren't any products associated to the article add the default value so then at query time
						//    based on the site setting it will add these to the result or not. 
						var productsDocument = document.GetField(FixedFacetsConfiguration.ProductFieldFacetName);
						if (productsDocument == null)
						{
							document.Add(
								new Field(
									FixedFacetsConfiguration.ProductFieldFacetName,
									this._index.ProductAccessDefaultValue,
									Field.Store.NO,
									Field.Index.NOT_ANALYZED));
						}

					}
				}

				if (!languageValueAdded)
				{
					document.Add(new Field(_index.LanguageLocaleCodeFieldName, _index.LanguageLocaleCodeDefaultValue, Field.Store.YES, Field.Index.NOT_ANALYZED));
				}

				// Add the field for the main, analyzed, search content.
				document.Add(new Field(_index.ContentFieldName, content.ToString(), _index.StoreContentField ? Field.Store.YES : Field.Store.NO, Field.Index.ANALYZED));

				if (_index.AddScopeField)
				{
					var scopeField = document.GetField(_index.ScopeValueSourceFieldName);

					var scopeValue = scopeField == null ? _index.ScopeDefaultValue : scopeField.StringValue;

					document.Add(new Field(_index.ScopeFieldName, scopeValue, Field.Store.NO, Field.Index.NOT_ANALYZED));
				}

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
				{
					this.AddDefaultWebRoleToAllDocumentsNotUnderCMS(document, entityMetadata.LogicalName);
					this.AddUrlDefinedToDocument(document, entityMetadata.LogicalName, fetchXmlResult);
				}

				var documentAnalyzer = lcid > 0
										   ? _index.GetLanguageSpecificAnalyzer(lcid)
										   : _index.Analyzer;
				return new CrmEntityIndexDocument(document, documentAnalyzer, primaryKey);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error: Exception when trying to create the index document. {0}", e));
			}
			return new CrmEntityIndexDocument(document, _index.Analyzer, primaryKey);
		}

		private static void AddDocumentFields(Document document, string fieldName, FetchXmlResultField fetchXmlField, AttributeMetadata attributeMetadata)
		{
			Guid id;

			// We want to normalize the formatting of Guids from what CRM returns, to the default framework
			// formatting, for easier querying later.
			if (AttributeTypeEqualsOneOf(attributeMetadata, "customer", "lookup", "uniqueidentifier") && Guid.TryParse(fetchXmlField.Value, out id))
			{
				document.Add(new Field(fieldName, id.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));

				return;
			}

			DateTime dateTimeValue;

			// Add additional sub-fields for datetimes, for easier queries based on date ranges.
			if (AttributeTypeEqualsOneOf(attributeMetadata, "datetime") && DateTime.TryParse(fetchXmlField.Value, out dateTimeValue))
			{
				document.Add(new Field("{0}.date".FormatWith(fieldName), dateTimeValue.ToString("yyyyMMdd"), Field.Store.NO, Field.Index.NOT_ANALYZED));
				document.Add(new Field("{0}.year".FormatWith(fieldName), dateTimeValue.ToString("yyyy"), Field.Store.NO, Field.Index.NOT_ANALYZED));
				document.Add(new Field("{0}.month".FormatWith(fieldName), dateTimeValue.ToString("MM"), Field.Store.NO, Field.Index.NOT_ANALYZED));
				document.Add(new Field("{0}.day".FormatWith(fieldName), dateTimeValue.ToString("dd"), Field.Store.NO, Field.Index.NOT_ANALYZED));
			}

			document.Add(new Field(fieldName, fetchXmlField.Value, Field.Store.NO, Field.Index.NOT_ANALYZED));
		}

		private static void AddDocumentFields(Document document, string fieldName, FetchXmlResultField fetchXmlField, AttributeMetadata attributeMetadata, ContentFieldBuilder content)
		{
			AddDocumentFields(document, fieldName, fetchXmlField, attributeMetadata);

			// If the field is some kind of text content, append it to the main analyzed search content.
			if (AttributeTypeEqualsOneOf(attributeMetadata, "string", "memo"))
			{
				IEnumerable<string> articleSections;

				if (TryGetKbArticleSections(attributeMetadata, fetchXmlField.Value, out articleSections))
				{
					foreach (var section in articleSections)
					{
						content.Append(section);
					}
				}
				else
				{
					content.Append(fetchXmlField.Value);
				}
			}
		}

		/// <summary>
		/// Adds the given web roles to document.
		/// </summary>
		/// <param name="document">
		/// The document.
		/// </param>
		/// <param name="roleNames">
		/// The web role names.
		/// </param>
		private void AddWebRolesToDocument(Document document, IEnumerable<string> roleNames)
		{
			if (roleNames != null)
			{
				foreach (var rule in roleNames)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("Adding rule: {0}", rule));

					document.Add(
						new Field(this._index.WebRoleFieldName, rule, Field.Store.NO, Field.Index.NOT_ANALYZED));
				}
			}
		}

		/// <summary>
		/// Adds the default web role to all documents not under cms.
		/// </summary>
		/// <param name="document">
		/// The document.
		/// </param>
		/// <param name="entityLogicalName">
		/// The entity logical name.
		/// </param>
		private void AddDefaultWebRoleToAllDocumentsNotUnderCMS(Document document, string entityLogicalName)
		{
			var cmsEntities = new[]
								  {
									  "adx_blog", "adx_blogpost",
									  "adx_communityforum", "adx_communityforumthread", "adx_communityforumpost",
									  "adx_idea", "adx_ideaforum",
									  "adx_webpage",
									  "adx_webfile"
								  };
			if (!cmsEntities.Contains(entityLogicalName))
			{
				document.Add(
					new Field(this._index.WebRoleFieldName, this._index.WebRoleDefaultValue, Field.Store.NO, Field.Index.NOT_ANALYZED));
			}

		}

		private void AddUrlDefinedToDocument(Document document, string entityLogicalName, FetchXmlResult fetchXmlResult)
		{
			var isUrlDefined = false;

			if (entityLogicalName == "adx_blog")
			{
				var blogPartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_partialurl");
				var parentPageIdFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_parentpageid");
				if (blogPartialUrlFetch == null || parentPageIdFetch == null)
				{
					return;
				}

				var blogPartialUrl = blogPartialUrlFetch.Value;
				isUrlDefined = CmsIndexHelper.IsWebPageUrlDefined(
					this._contentMapProvider,
					new Guid(parentPageIdFetch.Value),
					blogPartialUrl);

			}
			if (entityLogicalName == "adx_blogpost")
			{
				var blogPostPartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_partialurl");
				var blogPartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_blog_blogpost.adx_partialurl");
				var parentWebPageIdFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_blog_blogpost.adx_parentpageid");
				if (blogPartialUrlFetch == null || parentWebPageIdFetch == null)
				{
					return;
				}
				var blogPostId = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_blogpostid");
				if (blogPostPartialUrlFetch == null && blogPostId == null)
				{
					return;
				}
				var blogPostPartialUrl = blogPostPartialUrlFetch == null ? blogPostId.Value : blogPostPartialUrlFetch.Value;
				var blogPartialUrl = blogPartialUrlFetch.Value;

				var blogPostsCombineUrl = string.Format("{0}/{1}", blogPartialUrl, blogPostPartialUrl);

				isUrlDefined = CmsIndexHelper.IsWebPageUrlDefined(
					this._contentMapProvider,
					new Guid(parentWebPageIdFetch.Value),
					blogPostsCombineUrl);
			}

			if (entityLogicalName == "adx_communityforum")
			{
				var forumIdFetch =
					fetchXmlResult.FirstOrDefault(x => x.Name == "adx_communityforumid");
				if (forumIdFetch == null)
				{
					return;
				}
				isUrlDefined = CmsIndexHelper.IsForumUrlDefined(
					this._contentMapProvider,
					new Guid(forumIdFetch.Value));
			}
			if (entityLogicalName == "adx_communityforumthread")
			{
				var forumIdFetch =
					fetchXmlResult.FirstOrDefault(x => x.Name == "adx_forumid");
				if (forumIdFetch == null)
				{
					return;
				}
				isUrlDefined = CmsIndexHelper.IsForumUrlDefined(
					this._contentMapProvider,
					new Guid(forumIdFetch.Value));
			}
			if (entityLogicalName == "adx_communityforumpost")
			{
				var forumIdFetch =
					fetchXmlResult.FirstOrDefault(x => x.Name == "adx_communityforumpost_communityforumthread.adx_forumid");
				if (forumIdFetch == null)
				{
					return;
				}
				isUrlDefined = CmsIndexHelper.IsForumUrlDefined(
					this._contentMapProvider,
					new Guid(forumIdFetch.Value));
			}

			if (entityLogicalName == "adx_idea")
			{
				var ideaPartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_partialurl");
				var ideaForumPartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_idea_ideaforum.adx_partialurl");
				if (ideaPartialUrlFetch == null || ideaForumPartialUrlFetch == null)
				{
					return;
				}
				var ideaPartialUrl = ideaPartialUrlFetch.Value;
				var ideaForumPartialUrl = ideaForumPartialUrlFetch.Value;

				isUrlDefined = !string.IsNullOrEmpty(ideaPartialUrl) && !string.IsNullOrEmpty(ideaForumPartialUrl);
			}
			if (entityLogicalName == "adx_ideaforum")
			{
				var ideaForumPartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_partialurl");
				if (ideaForumPartialUrlFetch == null)
				{
					return;
				}
				isUrlDefined = !string.IsNullOrEmpty(ideaForumPartialUrlFetch.Value);
			}

			if (entityLogicalName == "incident")
			{
				isUrlDefined = CmsIndexHelper.IsSiteMakerUrlDefined(this._contentMapProvider, "Case");
			}

			if (entityLogicalName == "adx_webfile")
			{
				var webfilePartialUrlFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_partialurl");
				var webPageIdFetch = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_parentpageid");
				if (webfilePartialUrlFetch == null || webPageIdFetch == null)
				{
					return;
				}
				isUrlDefined = CmsIndexHelper.IsWebPageUrlDefined(
					this._contentMapProvider,
					new Guid(webPageIdFetch.Value),
					webfilePartialUrlFetch.Value);
			}

			if (entityLogicalName == "adx_webpage")
			{
				var webpageId = fetchXmlResult.FirstOrDefault(x => x.Name == "adx_webpageid");
				if (webpageId == null)
				{
					return;
				}
				var primaryId = new Guid(webpageId.Value);
				isUrlDefined = CmsIndexHelper.IsWebPageUrlDefined(this._contentMapProvider, primaryId);
			}

			if (!this.oOBUrlDefinedEntities.Contains(entityLogicalName))
			{
				isUrlDefined = true;
			}

			document.Add(
						new Field(this._index.IsUrlDefinedFieldName, isUrlDefined.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
		}

		private static bool AttributeTypeEqualsOneOf(AttributeMetadata attributeMetadata, params string[] typeNames)
		{
			if (attributeMetadata == null || attributeMetadata.AttributeType == null)
			{
				return false;
			}

			var attributeTypeName = attributeMetadata.AttributeType.Value.ToString();

			return typeNames.Any(name => string.Equals(attributeTypeName, name, StringComparison.InvariantCultureIgnoreCase));
		}

		private static bool TryGetKbArticleSections(AttributeMetadata attributeMetadata, string attributeValue, out IEnumerable<string> sections)
		{
			sections = null;

			if (attributeMetadata == null || attributeMetadata.EntityLogicalName != "kbarticle" || attributeMetadata.LogicalName != "articlexml")
			{
				return false;
			}

			try
			{
				var articleXml = XDocument.Parse(attributeValue);

				sections = articleXml.XPathSelectElements("//section").Select(e => e.Value);

				return true;
			}
			catch (XmlException)
			{
				return false;
			}
		}
		private string GetNotesFilterPrefix()
		{
			var prefix =
			_dataContext.CreateQuery("adx_sitesetting")
				.Where(s => s.GetAttributeValue<string>("adx_name") == "KnowledgeManagement/NotesFilter")
				.Select(v => v.GetAttributeValue<string>("adx_value"))
				.FirstOrDefault();

			return prefix ?? string.Empty;
		}
	}
}
