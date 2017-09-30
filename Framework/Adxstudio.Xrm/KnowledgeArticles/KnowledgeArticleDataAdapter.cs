/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Web;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Feedback;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Adxstudio.Xrm.Text;
	using Microsoft.Crm.Sdk.Messages;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Notes;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.UI.WebForms;

	/// <summary>
	/// Provides data operations for a single knowledge article, as represented by a KnowledgeArticle entity.
	/// </summary>
	public class KnowledgeArticleDataAdapter : IKnowledgeArticleDataAdapter, ICommentDataAdapter
	{
		private bool? _hasCommentModerationPermission;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="article">The article to get and set data for.</param>
		/// <param name="code">Article language code</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public KnowledgeArticleDataAdapter(EntityReference article, string code, string portalName = null) : this(article, code, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="article">The article to get and set data for.</param>
		/// <param name="code">Article language code</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public KnowledgeArticleDataAdapter(Entity article, string code = "", string portalName = null) : this(article.ToEntityReference(), code, portalName) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="article">Knowledge Article Entity Reference</param>
		/// <param name="code">Article language code</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public KnowledgeArticleDataAdapter(EntityReference article, string code, IDataAdapterDependencies dependencies)
		{
			article.ThrowOnNull("article");
			article.AssertLogicalName("knowledgearticle");
			dependencies.ThrowOnNull("dependencies");

			this.KnowledgeArticle = article;
			this.Dependencies = dependencies;
			this.LanguageCode = code;
		}

		public virtual IKnowledgeArticle Select()
		{
			var article = GetArticleEntity(Dependencies.GetServiceContext());

			return article == null
				? null
				: new KnowledgeArticleFactory(Dependencies).Create(new[] { article }).FirstOrDefault();
		}

		public IEnumerable<IRelatedArticle> SelectRelatedArticles(IEnumerable<EntityCollection> entityCollections)
		{
			var articleCollection = entityCollections.FirstOrDefault(e => e.EntityName.Equals("knowledgearticle"));
			return this.ToRelatedArticles(articleCollection);
		}

		public virtual IEnumerable<IRelatedProduct> SelectRelatedProducts(IEnumerable<EntityCollection> entityCollections)
			{
			var productCollection = entityCollections.FirstOrDefault(e => e.EntityName.Equals("product"));
			return this.ToRelatedProducts(productCollection);
			}

		public IEnumerable<EntityCollection> GetRelatedProductsAndArticles(IKnowledgeArticle article)
		{
			var serviceContext = this.Dependencies.GetServiceContext();
			Fetch fetchRelatedProducts = null;
			Fetch fetchRelatedArticles = null;

			var articleConnectionRole = GetArticleConnectionRole(serviceContext, this.KnowledgeArticleConnectionRoleId);
			var relatedProductConnectionRole = GetArticleConnectionRole(serviceContext, this.RelatedProductConnectionRoleId);

			if (articleConnectionRole != null && relatedProductConnectionRole != null)
			{
				var relatedProductsFetchXml = string.Format(RelatedProductsFetchXmlFormat, 
					article.Id,
					relatedProductConnectionRole.Id, 
					articleConnectionRole.Id);

				fetchRelatedProducts = Fetch.Parse(relatedProductsFetchXml);
			}

			var languageCondition = string.Empty;
			if (!string.IsNullOrWhiteSpace(this.LanguageCode))
			{
				languageCondition = "<condition entityname='language_locale' attribute='code' operator='eq' value = '" + this.LanguageCode + "' />";
			}

			var primaryArticleConnectionRole = GetArticleConnectionRole(serviceContext, this.PrimaryArticleConnectionRoleId);
			var relatedArticleConnectionRole = GetArticleConnectionRole(serviceContext, this.RelatedArticleConnectionRoleId);

			if (primaryArticleConnectionRole != null && relatedArticleConnectionRole != null)
			{
				var id = article.RootArticle == null ? article.Id : article.RootArticle.Id;
				var relatedArticlesFetchXml = string.Format(RelatedArticlesFetchXmlFormat, 
					id, 
					primaryArticleConnectionRole.Id,
					relatedArticleConnectionRole.Id, 
					languageCondition);

				fetchRelatedArticles = Fetch.Parse(relatedArticlesFetchXml);
			}

			var products = new EntityCollection();
			if (fetchRelatedProducts != null)
			{
				products = serviceContext.RetrieveMultiple(fetchRelatedProducts, RequestFlag.AllowStaleData);
			}

			var articles = new EntityCollection();
			if (fetchRelatedArticles != null)
			{
				articles = serviceContext.RetrieveMultiple(fetchRelatedArticles, RequestFlag.AllowStaleData);
			}

			return new List<EntityCollection> { products, articles }.AsEnumerable();
		}

		private IEnumerable<IRelatedProduct> ToRelatedProducts(EntityCollection relatedProductsCollection)
			{
			var serviceContext = this.Dependencies.GetServiceContext();
			var urlProvider = this.Dependencies.GetUrlProvider();
			var relatedProducts = Enumerable.Empty<RelatedProduct>();
			if (relatedProductsCollection != null && relatedProductsCollection.Entities.Count > 0)
			{
				int lcid = 0;
				this.ProductLocalizationShouldOccur(out lcid);

				relatedProducts =
					relatedProductsCollection.Entities
					.Select(e => new { Id = e.Id, Name = e.GetAttributeValue<string>("name"), Url = urlProvider.GetUrl(serviceContext, e) })
						.Where(e => !(string.IsNullOrEmpty(e.Name)))
						.Select(e => new RelatedProduct(e.Id, this.LocalizeProductLabels(serviceContext, lcid, e.Id, e.Name), e.Url))
						.OrderBy(e => e.Name)
						.AsParallel();
			}

			return relatedProducts;
		}

		private IEnumerable<IRelatedArticle> ToRelatedArticles(EntityCollection relatedArticlesCollection)
		{
			var serviceContext = this.Dependencies.GetServiceContext();
			var securityProvider = this.Dependencies.GetSecurityProvider();
			var urlProvider = this.Dependencies.GetUrlProvider();
			var relatedArticles = Enumerable.Empty<RelatedArticle>();
			if (relatedArticlesCollection != null && relatedArticlesCollection.Entities.Count > 0)
			{
				int lcid = 0;
				this.ProductLocalizationShouldOccur(out lcid);

				relatedArticles =
					relatedArticlesCollection.Entities.Where(e => securityProvider.TryAssert(serviceContext, e, CrmEntityRight.Read))
						.Select(e => new { Title = e.GetAttributeValue<string>("title"), Url = urlProvider.GetUrl(serviceContext, e) })
						.Where(e => !(string.IsNullOrEmpty(e.Title) || string.IsNullOrEmpty(e.Url)))
						.Select(e => new RelatedArticle(e.Title, e.Url))
						.OrderBy(e => e.Title);
			}

			return relatedArticles;
		}

		public virtual IEnumerable<IRelatedNote> SelectRelatedNotes(IKnowledgeArticle article)
		{
			if (!IsAnnotationSearchEnabled)
			{
				return null;
			}

			var annotationDataAdapter = new AnnotationDataAdapter(this.Dependencies);
			var webPrefix = GetNotesFilterPrefix;

			var relatedNotes = annotationDataAdapter.GetDocuments(article.EntityReference, webPrefix: webPrefix);
			return relatedNotes.Select(a => new RelatedNote(a.NoteText == null ? string.Empty : a.NoteText.ToString().Substring(webPrefix.Length), a.FileAttachment.FileName, a.FileAttachment.Url));
		}

		/// <summary>
		/// Increments the View Count for the Knowledge Article by 1
		/// </summary>
		public void IncrementKnowledgeArticleViewCount(Uri urlReferrer)
		{
			if (!CaptureViewCountEnabled) return;

			if ((CaptureKnowledgeArticleReferrer) && (ReferrerEnabledCrmVersion))
			{
				var keyValueReferrerAndHost = GetReferrerTypeAndHost(urlReferrer);
				IncrementKnowledgeArticleReferrerViewCount((int)keyValueReferrerAndHost.Key, keyValueReferrerAndHost.Value);
			}
			else
			{
				var request = new IncrementKnowledgeArticleViewCountRequest()
				{
					Source = KnowledgeArticle,
					Count = 1,
					Location = KnowledgeArticleViewCountWebLocation,
					ViewDate = DateTime.Now
				};

				var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
				portalContext.ServiceContext.Execute(request);
			}
		}

		#region Private Methods
		/// <summary>
		/// Checks whether Product Localization should occur
		/// </summary>
		/// <param name="lcid">Lcid variable to instantiate for non-base requests</param>
		/// <returns>True, if localization of Product should occur</returns>
		private bool ProductLocalizationShouldOccur(out int lcid)
		{
			lcid = 0;

			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (!contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				return false;
			}

			// If the user's ContextLanguage is not different than the base language, do not localize since the localization would return same Title's anyways.
			var context = this.Dependencies.GetRequestContext().HttpContext;
			var organizationBaseLanguageCode = context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode;
			if (contextLanguageInfo.ContextLanguage.CrmLcid == organizationBaseLanguageCode)
			{
				return false;
			}

			lcid = contextLanguageInfo.ContextLanguage.CrmLcid;

			return true;
		}

		/// <summary>
		/// Replaces Category labels with localized labels where available using specified <paramref name="languageCode"/>
		/// </summary>
		/// <param name="context">Organization Service Context</param>
		/// <param name="languageCode">LCID for label request</param>
		/// <param name="productId">Product ID</param>
		/// <param name="fallbackProductLabel">base language Product label to fallback to</param>
		private string LocalizeProductLabels(OrganizationServiceContext context, int languageCode, Guid productId, string fallbackProductLabel)
		{
			// If localization shouldn't occur, default to fallback label
			if (languageCode == 0)
			{
				return fallbackProductLabel;
			}

			var localizedLabel = context.RetrieveLocalizedLabel(new EntityReference("product", productId), "name", languageCode);

			if (!string.IsNullOrWhiteSpace(localizedLabel))
			{
				return localizedLabel;
			}

			return fallbackProductLabel;
		}

		private KeyValuePair<ReferrerType, string> GetReferrerTypeAndHost(Uri urlReferrer)
		{
			var portalHostName = string.Empty;
			var url = GetPortalUrl("Search");

			if (urlReferrer == null)
			{
				if (url != null)
				{
					portalHostName = url.Host;
				}
				return new KeyValuePair<ReferrerType, string>(ReferrerType.DirectLink, portalHostName);
			}

			// GetPortalUrl(...) will return URL without language code prefix, so in order to make comparison work, need to (if necessary) strip out 
			// language code prefix from urlReferrer as well.
			string absoluteUri;
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				absoluteUri = contextLanguageInfo.StripLanguageCodeFromAbsolutePath(urlReferrer.PathAndQuery);
			}
			else
			{
				absoluteUri = urlReferrer.AbsoluteUri;
			}


			if (url != null)
			{
				portalHostName = url.Host;
				if (absoluteUri.Contains(url.Uri.ToString()))
				{
					return new KeyValuePair<ReferrerType, string>(ReferrerType.PortalSearch, url.Host);
				}
			}

			url = GetPortalUrl("Create Case");

			if (url != null)
			{
				portalHostName = url.Host;
				if (absoluteUri.Contains(url.Uri.ToString()))
				{
					return new KeyValuePair<ReferrerType, string>(ReferrerType.PortalCaseDeflectionSearch, url.Host);
				}
			}

			if (!string.IsNullOrEmpty(portalHostName))
			{
				if (absoluteUri.Contains(portalHostName))
				{
					return new KeyValuePair<ReferrerType, string>(ReferrerType.Browse, portalHostName);
				}
			}

			var regEx = new Regex(ExternalSearchEngineString, RegexOptions.IgnoreCase);
			var isSearch = regEx.Match(absoluteUri).Success;

			return isSearch ? new KeyValuePair<ReferrerType, string>(ReferrerType.ExternalSearchEngine, portalHostName) : new KeyValuePair<ReferrerType, string>(ReferrerType.ExternalWebsite, portalHostName);
		}

		private static UrlBuilder GetPortalUrl(string siteMarkerName)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var page = portalContext.ServiceContext.GetPageBySiteMarkerName(portalContext.Website, siteMarkerName);

			return page == null ? null : new UrlBuilder(portalContext.ServiceContext.GetUrl(page));
		}

		private bool ReferrerEnabledCrmVersion
		{
			get
			{
				var portalDetails = HttpContext.Current.GetPortalSolutionsDetails();
				var enabledVersion = new Version("8.2.0.0");

				return portalDetails != null && portalDetails.CrmVersion.CompareTo(enabledVersion) >= 0;
			}
		}

		/// <summary>
		/// Checks the Article/CaptureViewCount Site Setting to determine if capturing view count is enabled
		/// </summary>
		private bool CaptureViewCountEnabled
		{
			get
			{
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
				var captureViewCountEnabledString = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "KnowledgeManagement/Article/CaptureViewCount");

				//By default, if the site setting isn't configured - don't capture view count
				bool captureViewCountEnabled = false;

				bool.TryParse(captureViewCountEnabledString, out captureViewCountEnabled);

				return captureViewCountEnabled;
			}
		}

		private bool CaptureKnowledgeArticleReferrer
		{
			get
			{
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
				var captureKnowledgeArticleReferrerString = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "KnowledgeManagement/Analytics/CaptureKnowledgeArticleReferrer");

				bool captureKnowledgeArticleReferrerEnabled = false;

				bool.TryParse(captureKnowledgeArticleReferrerString, out captureKnowledgeArticleReferrerEnabled);

				return captureKnowledgeArticleReferrerEnabled;

			}
		}

		private bool IsAnnotationSearchEnabled
		{
			get
			{
				var adapter = new SettingDataAdapter(new PortalConfigurationDataAdapterDependencies(), HttpContext.Current.GetWebsite());
				return adapter.GetBooleanValue("KnowledgeManagement/DisplayNotes") ?? false;
			}
		}

		private string GetNotesFilterPrefix
		{
			get
			{
				return HttpContext.Current.GetSiteSetting("KnowledgeManagement/NotesFilter") ?? string.Empty;
			}
		}

		private string ExternalSearchEngineString
		{
			get
			{
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
				return portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "KnowledgeManagement/Analytics/ExternalSearchEngines");
			}
		}

		private Entity GetArticleEntity(OrganizationServiceContext serviceContext)
		{
			var article = serviceContext.RetrieveSingle("knowledgearticle",
				FetchAttribute.All,
				new Condition("knowledgearticleid", ConditionOperator.Equal, KnowledgeArticle.Id),
				false,
				false,
				RequestFlag.AllowStaleData);

			if (article == null)
			{
				throw new InvalidOperationException(
					ResourceManager.GetString("Knowledge_Article_NotFound").FormatWith(KnowledgeArticle.Id));
			}

			return article;
		}

		private static Entity GetArticleConnectionRole(OrganizationServiceContext serviceContext, Guid connectionRoleId)
		{
			var connectionRole = serviceContext.RetrieveSingle("connectionrole",
				FetchAttribute.All,
				new Condition("connectionroleid", ConditionOperator.Equal, connectionRoleId));

			return connectionRole;
		}

		#endregion Private methods

		public IDictionary<string, object> GetCommentAttributes(
			string content,
			string authorName = null,
			string authorEmail = null,
			string authorUrl = null,
			HttpContext context = null)
		{
			var article = Select();

			if (article == null)
			{
				throw new InvalidOperationException(
					ResourceManager.GetString("Knowledge_Article_With_EntityID_Load_Exception").FormatWith(KnowledgeArticle.Id));
			}

			var postedOn = DateTime.UtcNow;

			var attributes = new Dictionary<string, object>
			{
				{ "regardingobjectid", KnowledgeArticle },
				{ "createdon", postedOn },
				{ "title", StringHelper.GetCommentTitleFromContent(content) },
				{ "adx_createdbycontact", authorName },
				{ "adx_contactemail", authorEmail },
				{ "adx_approved", article.CommentPolicy == CommentPolicy.Open || article.CommentPolicy == CommentPolicy.OpenToAuthenticatedUsers },
				{ "comments", content },
			};

			var portalUser = Dependencies.GetPortalUser();

			if (portalUser != null && portalUser.LogicalName == "contact")
			{
				attributes[FeedbackMetadataAttributes.UserIdAttributeName] = portalUser;
			}
			else if (context != null && context.Profile != null)
			{
				attributes[FeedbackMetadataAttributes.VisitorAttributeName] = context.Profile.UserName;
			}

			return attributes;
		}

		/// <summary>
		/// Returns comments that have been posted for the article this adapter applies to.
		/// </summary>
		public virtual IEnumerable<IComment> SelectComments()
		{
			return SelectComments(0);
		}

		/// <summary>
		/// Returns comments that have been posted for the article this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		public virtual IEnumerable<IComment> SelectComments(int startRowIndex, int maximumRows = -1)
		{
			var comments = new List<Comment>();
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) || maximumRows == 0)
			{
				return comments;
			}
			var query =
				Cms.OrganizationServiceContextExtensions.SelectCommentsByPage(
					Cms.OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows), KnowledgeArticle.Id,
					true, ChronologicalComments);
			var commentsEntitiesResult = Dependencies.GetServiceContext().RetrieveMultiple(query);
			comments.AddRange(
				commentsEntitiesResult.Entities.Select(
					commentEntity =>
						new Comment(commentEntity,
							new Lazy<ApplicationPath>(() => Dependencies.GetEditPath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<ApplicationPath>(() => Dependencies.GetDeletePath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<bool>(() => true, LazyThreadSafetyMode.None))));
			return comments;
		}

		/// <summary>
		/// Returns the number of comments that have been posted for the article this adapter applies to.
		/// </summary>
		public virtual int SelectCommentCount()
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return 0;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedComments = TryAssertCommentModerationPermission(serviceContext);

			return serviceContext.FetchCount("feedback", "feedbackid", addCondition =>
			{
				addCondition("regardingobjectid", "eq", KnowledgeArticle.Id.ToString());
				addCondition("statecode", "eq", "0");

				if (!includeUnapprovedComments)
				{
					addCondition("adx_approved", "eq", "true");
				}
			},
			null,
			addBinaryFilterCondition =>
			{
				addBinaryFilterCondition("comments", "not-null");
			});
		}

		protected virtual bool TryAssertCommentModerationPermission(OrganizationServiceContext serviceContext)
		{
			if (_hasCommentModerationPermission.HasValue)
			{
				return _hasCommentModerationPermission.Value;
			}

			var security = Dependencies.GetSecurityProvider();

			_hasCommentModerationPermission = security.TryAssert(serviceContext, GetKnowledgeArticleEntity(serviceContext), CrmEntityRight.Change);

			return _hasCommentModerationPermission.Value;
		}

		private Entity GetKnowledgeArticleEntity(OrganizationServiceContext serviceContext)
		{
			var article = serviceContext.RetrieveSingle("knowledgearticle",
				FetchAttribute.All,
				new Condition("knowledgearticleid", ConditionOperator.Equal, KnowledgeArticle.Id),
				false,
				false,
				RequestFlag.AllowStaleData);

			if (article == null)
			{
				throw new InvalidOperationException(string.Format("Can't find {0} having ID {1}.", "knowledgearticle", KnowledgeArticle.Id));
			}

			return article;
		}

		public string GetCommentLogicalName()
		{
			return "feedback";
		}

		public string GetCommentContentAttributeName()
		{
			return "comments";
		}

		public ICommentPolicyReader GetCommentPolicyReader()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Post a comment for the knowledge article this adapter applies to.
		/// </summary>
		/// <param name="content">The comment copy.</param>
		/// <param name="authorName">The name of the author for this comment (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for this comment (ignored if user is authenticated).</param>
		public virtual void CreateComment(string content, string authorName = null, string authorEmail = null)
		{
			content.ThrowOnNullOrWhitespace("content");

			var title = StringHelper.GetCommentTitleFromContent(content);
			title.ThrowOnNullOrWhitespace("title");

			var author = Dependencies.GetPortalUser();

			if (author == null)
			{
				authorName.ThrowOnNullOrWhitespace("authorName",
					ResourceManager.GetString("ErrorInCreatingKnowledgeArticleComment_WithNullOrWhitespace"));
				authorEmail.ThrowOnNullOrWhitespace("authorEmail",
					ResourceManager.GetString("ErrorInCreatingKnowledgeArticleComment_WithNullOrWhitespace"));
			}

			var article = Select();

			var feedback = new Entity("feedback");

			feedback["title"] = title;
			feedback["comments"] = content;
			feedback["regardingobjectid"] = article.EntityReference;
			feedback["adx_createdbycontact"] = authorName;
			feedback["adx_contactemail"] = authorEmail;
			feedback["adx_approved"] = article.CommentPolicy != CommentPolicy.Moderated;
			feedback["source"] = new OptionSetValue((int)FeedbackSource.Portal);

			if (author != null)
			{
				feedback["createdbycontact"] = author;
			}

			var context = Dependencies.GetServiceContextForWrite();

			context.AddObject(feedback);
			context.SaveChanges();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.KnowledgeArticle, HttpContext.Current, "create_comment_article", 1, feedback.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Creates or Updates Case Deflection Entity for the knowledgearticle.
		/// </summary>
		/// <param name="title">The title of article</param>
		/// <param name="searchText">The deflection SearchText of Article</param>
		/// <param name="isRatingEnabled">The KnowledgeArticle RatingEnabled</param>
		/// <param name="context">The OrganizationServiceContext</param>
		public virtual void CreateUpdateCaseDeflection(string title, string searchText, bool isRatingEnabled, OrganizationServiceContext context)
		{
			var author = Dependencies.GetPortalUser();

			// Update if applicable
			if (author != null)
			{
				var existing = context.CreateQuery("adx_casedeflection").FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_knowledgearticle") == KnowledgeArticle.Id && e.GetAttributeValue<Guid>("adx_contact") == author.Id);

				if (existing != null)
				{
					UpdateCaseDeflection(existing, title, searchText);
				}
				else
				{
					CreateCaseDeflection(title, searchText);
				}

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) && isRatingEnabled)
				{
					CreateUpdateRating(Rating, MaxRating, MinRating);
				}
			}
		}

		/// <summary>
		/// Creates Case Deflection Entity for the knowledgearticle.
		/// </summary>
		/// <param name="title">The title of article</param>
		/// <param name="searchText">The searchText of article</param>
		public virtual void CreateCaseDeflection(string title, string searchText)
		{
			var author = Dependencies.GetPortalUser();

			var casedeflection = new Entity("adx_casedeflection");
			casedeflection["adx_casetitle"] = searchText;
			casedeflection["adx_name"] = title;
			casedeflection["adx_contact"] = author;
			casedeflection["adx_knowledgearticle"] = KnowledgeArticle;

			var context = Dependencies.GetServiceContextForWrite();
			context.AddObject(casedeflection);
			context.SaveChanges();
		}

		/// <summary>
		/// Updates Case Deflection Entity for the knowledgearticle.
		/// </summary>
		/// <param name="existingRecord">The Casedeflection record</param>
		/// <param name="title">The title of knowledgeArticle</param>
		/// <param name="searchText">The searchText of knowledgeArticle</param>
		public virtual void UpdateCaseDeflection(Entity existingRecord, string title, string searchText)
		{
			var author = Dependencies.GetPortalUser();
			Entity recordToUpdate = new Entity(existingRecord.LogicalName) { Id = existingRecord.Id };
			recordToUpdate.Attributes[CaseDeflectionMetadataAttributes.CaseTitle] = searchText;
			recordToUpdate.Attributes[CaseDeflectionMetadataAttributes.NameOfCaseDeflection] = title;
			recordToUpdate.Attributes[CaseDeflectionMetadataAttributes.ContactName] = author;
			recordToUpdate.Attributes[CaseDeflectionMetadataAttributes.KnowledgearticleIdAttributeName] = KnowledgeArticle;

			var context = Dependencies.GetServiceContextForWrite();
			if (!context.IsAttached(recordToUpdate))
			{
				context.Attach(recordToUpdate);
			}

			context.UpdateObject(recordToUpdate);
			context.SaveChanges();
		}

		/// <summary>
		/// Vote for the knowledge article this adapter applies to.
		/// Creates or updates the current user's rating
		/// </summary>
		/// <param name="rating">Rating value.</param>
		/// <param name="maxRating">Maximum rating value.</param>
		/// <param name="minRating">Minimum rating value.</param>
		public virtual void CreateUpdateRating(int rating, int maxRating, int minRating, string visitorID = null)
		{
			var author = Dependencies.GetPortalUser();

			// Update if applicable
			var article = Select();
			var context = Dependencies.GetServiceContextForWrite();
			Entity existing = null;

			if (author != null)
			{
				existing = context.CreateQuery("feedback")
					.FirstOrDefault(feedbackRecord => feedbackRecord["regardingobjectid"] == article.EntityReference
														&& feedbackRecord["createdbycontact"] == author);

			}
			else if (!string.IsNullOrEmpty(visitorID))
			{
				existing = context.CreateQuery("feedback")
					.FirstOrDefault(feedbackRecord => feedbackRecord["regardingobjectid"] == article.EntityReference
														&& feedbackRecord.GetAttributeValue<string>(FeedbackMetadataAttributes.VisitorAttributeName) == visitorID);
			}

			if (existing != null)
			{
				UpdateRating(existing, rating, maxRating, minRating);
				return;
			}

			CreateRating(rating, maxRating, minRating, visitorID);
		}

		/// <summary>
		/// Vote for the knowledge article this adapter applies to.
		/// </summary>
		/// <param name="rating">Rating value.</param>
		/// <param name="maxRating">Maximum rating value.</param>
		/// <param name="minRating">Minimum rating value.</param>
		private void CreateRating(int rating, int maxRating, int minRating, string visitorID = null)
		{
			var article = Select();
			var author = Dependencies.GetPortalUser();
			var context = Dependencies.GetServiceContextForWrite();
			var articleMetadata = GetArticleEntityMetadata();

			var displayName = articleMetadata != null ? articleMetadata.DisplayName.UserLocalizedLabel.Label : string.Empty;

			var feedback = new Entity("feedback");

			feedback["title"] = ResourceManager.GetString("Feedback_Default_Title").FormatWith(displayName, article.Title);
			feedback["rating"] = rating;
			feedback["minrating"] = minRating;
			feedback["maxrating"] = maxRating;
			feedback["regardingobjectid"] = article.EntityReference;
			feedback["adx_approved"] = true;
			feedback["source"] = new OptionSetValue((int)FeedbackSource.Portal);

			if (author != null)
			{
				feedback["createdbycontact"] = author;
			}
			else if (!string.IsNullOrEmpty(visitorID))
			{
				feedback.Attributes[FeedbackMetadataAttributes.VisitorAttributeName] = visitorID;
			}

			context.AddObject(feedback);
			context.SaveChanges();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.KnowledgeArticle, HttpContext.Current, "create_rating_article", 1, feedback.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Updates the given rating record with the new information
		/// </summary>
		/// <param name="rating">Rating value.</param>
		/// <param name="maxRating">Maximum rating value.</param>
		/// <param name="minRating">Minimum rating value.</param>
		private void UpdateRating(Entity existingRecord, int rating, int maxRating, int minRating)
		{
			Entity recordToUpdate = new Entity(existingRecord.LogicalName) { Id = existingRecord.Id };
			recordToUpdate.Attributes[FeedbackMetadataAttributes.RatingValueAttributeName] = rating;
			recordToUpdate.Attributes[FeedbackMetadataAttributes.MaxRatingAttributeName] = maxRating;
			recordToUpdate.Attributes[FeedbackMetadataAttributes.MinRatingAttributeName] = minRating;

			var context = Dependencies.GetServiceContextForWrite();

			if (!context.IsAttached(recordToUpdate))
			{
				context.Attach(recordToUpdate);
			}

			context.UpdateObject(recordToUpdate);
			context.SaveChanges();
		}

		private void IncrementKnowledgeArticleReferrerViewCount(int referrer, string domainName)
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();

			Condition[] conditions = {
				new Condition("knowledgearticleid", ConditionOperator.Equal, KnowledgeArticle.Id),
				new Condition("viewdate", ConditionOperator.Today),
				new Condition("adx_referrer", ConditionOperator.Equal, referrer)
				};

			var list = conditions.ToList();

			if (!string.IsNullOrEmpty(domainName))
			{

				list.Add(new Condition("adx_domainname", ConditionOperator.Equal, domainName));
			}
			var kbViewsfetch = new Fetch
			{
				Distinct = true,
				Entity = new FetchEntity
				{
					Name = "knowledgearticleviews",
					Attributes = new List<FetchAttribute>
					{
						new FetchAttribute("knowledgearticleviewsid"),
						new FetchAttribute("knowledgearticleview"),
					},
					Filters = new[]
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = list
						}
					},
				}
			};

			var kbViewsEntity = kbViewsfetch.Execute(serviceContext as IOrganizationService, RequestFlag.AllowStaleData).Entities.FirstOrDefault();


			if (kbViewsEntity == null)
			{
				var knowledgeArticleView = new Entity("knowledgearticleviews") { Id = Guid.NewGuid() };
				knowledgeArticleView["knowledgearticleid"] = KnowledgeArticle;
				knowledgeArticleView["viewdate"] = DateTime.Now;
				knowledgeArticleView["adx_referrer"] = new OptionSetValue(referrer);
				knowledgeArticleView["location"] = new OptionSetValue(KnowledgeArticleViewCountWebLocation);
				knowledgeArticleView["knowledgearticleview"] = 1;
				knowledgeArticleView["adx_domainname"] = string.IsNullOrEmpty(domainName) ? null : domainName;

				serviceContext.AddObject(knowledgeArticleView);
				serviceContext.SaveChanges();
			}
			else
			{
				var updateKnowledgeArticleView = new Entity("knowledgearticleviews") { Id = kbViewsEntity.Id };
				updateKnowledgeArticleView["viewdate"] = DateTime.Now;
				updateKnowledgeArticleView["knowledgearticleview"] = (int)kbViewsEntity.Attributes["knowledgearticleview"] + 1;

				var updateServiceContext = Dependencies.GetServiceContextForWrite();

				if (!updateServiceContext.IsAttached(updateKnowledgeArticleView))
				{
					updateServiceContext.Attach(updateKnowledgeArticleView);
				}

				updateServiceContext.UpdateObject(updateKnowledgeArticleView);
				updateServiceContext.SaveChanges();
			}
		}

		/// <summary>
		/// Retrieve Knowledge Article EntityMetadata
		/// </summary>
		/// <returns>Knowledge Article EntityMetadata</returns>
		private EntityMetadata GetArticleEntityMetadata()
		{
			var context = Dependencies.GetServiceContext();

			var metadataRequest = new RetrieveEntityRequest()
			{
				EntityFilters = EntityFilters.All,
				LogicalName = KnowledgeArticle.LogicalName,
				RetrieveAsIfPublished = false
			};

			return ((RetrieveEntityResponse)context.Execute(metadataRequest)).EntityMetadata;
		}

		protected IDataAdapterDependencies Dependencies { get; set; }

		protected EntityReference KnowledgeArticle { get; set; }

		protected string LanguageCode { get; set; }

		protected IOrganizationService PortalOrganizationService
		{
			get { return Dependencies.GetRequestContext().HttpContext.GetOrganizationService(); }
		}

		public bool? ChronologicalComments { get; set; }

		private const int KnowledgeArticleViewCountWebLocation = 2;

		enum ReferrerType
		{
			Browse = 1,
			PortalSearch = 2,
			ExternalSearchEngine = 3,
			ExternalWebsite = 4,
			PortalCaseDeflectionSearch = 5,
			DirectLink = 6
		}

		private const int Rating = 5;

		private const int MaxRating = 5;

		private const int MinRating = 0;

		private Guid KnowledgeArticleConnectionRoleId = new Guid("81BB2655-F19B-42B2-9C4B-D45B84C3F61C");
		private Guid RelatedProductConnectionRoleId = new Guid("131F5D06-9F36-4B59-B8B7-A1F7D6C5C5EF");
		private Guid PrimaryArticleConnectionRoleId = new Guid("5A18DFC8-0B8B-40C7-9381-CCE1C485822D");
		private Guid RelatedArticleConnectionRoleId = new Guid("CFFE4A59-CE11-4FCA-B132-5985D3917D26");
		private const string RelatedProductsFetchXmlFormat = @"
				<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
					<entity name='product'>
						<attribute name='productid' />
						<attribute name='name' />
						<attribute name='productnumber' />
						<attribute name='description' />
						<attribute name='statecode' />
						<order attribute='productnumber' descending='false' />
						<link-entity name='connection' from='record2id' to='productid' alias='ad'>
							<filter type='and'>
								<condition attribute='record1id' operator='eq' value='{0}' />
								<condition attribute='record2roleid' operator='eq' uiname='Associated Product' uitype='connectionrole' value='{1}' />
								<condition attribute='record1roleid' operator='eq' uiname='Knowledge Article' uitype='connectionrole' value='{2}' />
							</filter>
						</link-entity>
						<filter type='and'>
							<condition attribute='statecode' operator='eq' value='0' />
						</filter>
					</entity>
				</fetch>";

		private const string RelatedArticlesFetchXmlFormat = @"
				<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
					<entity name='knowledgearticle'>
						<attribute name='articlepublicnumber' />
						<attribute name='knowledgearticleid' />
						<attribute name='title' />
						<attribute name='keywords' />
						<attribute name='createdon' />
						<attribute name='statecode' />
						<attribute name='statuscode' />
						<attribute name='isinternal' />
						<attribute name='isrootarticle' />
						<attribute name='knowledgearticleviews' />
						<attribute name='languagelocaleid' />
						<order attribute='articlepublicnumber' descending='false' />
						<link-entity name='languagelocale' from='languagelocaleid' to='languagelocaleid' visible='false' link-type='outer'  alias='language_locale'>
							<attribute name='localeid' />
							<attribute name='code' />
							<attribute name='region' />
							<attribute name='name' />
							<attribute name='language' />
						</link-entity>
						<filter type='and'>
							<condition attribute='isrootarticle' operator='eq' value='0' />
							<condition attribute='statecode' operator='eq' value='3' />
							<condition attribute='isinternal' operator='eq' value='0' />
							{3}
						</filter>
						<link-entity name='connection' from='record2id' to='knowledgearticleid' alias='aj'>
							<filter type='and'>
								<condition attribute='record1id' operator='eq' value='{0}' />
								<condition attribute='record1roleid' operator='eq' uiname='Primary Article' uitype='connectionrole' value='{1}' />
								<condition attribute='record2roleid' operator='eq' uiname='Related Article' uitype='connectionrole' value='{2}' />
							</filter>
						</link-entity>
					</entity>
				</fetch>";
	}
}
