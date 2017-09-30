/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Web.UI.WebForms;
using Adxstudio.Xrm.Services;

namespace Adxstudio.Xrm.Blogs
{
	public static class OrganizationServiceContextExtensions
	{
		public static IQueryable<Entity> GetAllBlogPostsInWebsite(this OrganizationServiceContext serviceContext, Guid websiteId)
		{
			// If multi-language is enabled, only select blog posts of blogs that are language-agnostic or match the current language.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			var query = contextLanguageInfo.IsCrmMultiLanguageEnabled ?
				from post in serviceContext.CreateQuery("adx_blogpost")
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<EntityReference>("adx_blogid").Id equals blog.GetAttributeValue<Guid>("adx_blogid")
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") != null && blog.GetAttributeValue<EntityReference>("adx_websiteid").Id == websiteId 
					where blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null || blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id
					where post.GetAttributeValue<EntityReference>("adx_blogid") != null && post.GetAttributeValue<bool?>("adx_published") == true
					orderby post.GetAttributeValue<DateTime?>("adx_date") descending
					select post 
				:
				from post in serviceContext.CreateQuery("adx_blogpost")
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<EntityReference>("adx_blogid").Id equals blog.GetAttributeValue<Guid>("adx_blogid")
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") != null && blog.GetAttributeValue<EntityReference>("adx_websiteid").Id == websiteId
					where post.GetAttributeValue<EntityReference>("adx_blogid") != null && post.GetAttributeValue<bool?>("adx_published") == true
					orderby post.GetAttributeValue<DateTime?>("adx_date") descending
					select post;
			return query;
		}

		public static IDictionary<Guid, int> FetchBlogPostCommentCounts(this OrganizationServiceContext serviceContext, IEnumerable<Guid> postIds)
		{
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return serviceContext.FetchCounts(
					"feedback",
					"feedbackid",
					"adx_blogpost",
					"adx_blogpostid",
					"regardingobjectid",
					postIds,
					addCondition => addCondition("adx_approved", "eq", "true"));
			}
			else
			{
				return new Dictionary<Guid, int>();
			}
		}

		public static int FetchBlogPostCount(this OrganizationServiceContext serviceContext, Guid blogId, bool published = true)
		{
			return FetchBlogPostCount(serviceContext, blogId, _ => { }, published);
		}

		public static int FetchBlogPostCount(this OrganizationServiceContext serviceContext, Guid blogId, string tag, bool published = true)
		{
			if (string.IsNullOrWhiteSpace(tag))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "tag");
			}

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_blogpostid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""adx_blogid"" operator=""eq"" />
						</filter>
						<filter type=""or"">
							<condition attribute=""adx_published"" operator=""eq"" value=""true"" />						
						</filter>
						<link-entity name=""adx_blogpost_tag"" from=""adx_blogpostid"" to=""adx_blogpostid"">
							<link-entity name=""adx_tag"" from=""adx_tagid"" to=""adx_tagid"">
								<filter type=""and"">
									<condition attribute=""adx_name"" operator=""eq"" />
								</filter>
							</link-entity>
						</link-entity>
					</entity>
				</fetch>");

			var blogIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_blogid']");

			if (blogIdAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_blogid filter"));
			}

			blogIdAttribute.SetAttributeValue("value", blogId.ToString());

			if (!published)
			{
				var publishedFilter = fetchXml.XPathSelectElement("//entity/filter[@type='or']");

				if (publishedFilter == null)
				{
					throw new InvalidOperationException("Unable to select the blog post publishing filter element.");
				}

				publishedFilter.Remove();
			}

			var tagNameCondition = fetchXml.XPathSelectElement("//link-entity[@name='adx_tag']/filter/condition[@attribute='adx_name']");

			if (tagNameCondition == null)
			{
				throw new InvalidOperationException("Unable to select the tag name filter element.");
			}

			tagNameCondition.SetAttributeValue("value", tag);

			var entity = serviceContext.RetrieveSingle(Fetch.Parse(fetchXml.ToString()));

			return (int)entity.GetAttributeValue<AliasedValue>("count").Value;
		}

		public static int FetchBlogPostCount(this OrganizationServiceContext serviceContext, Guid blogId, Action<Action<string, string, string>> addFilterConditions, bool published = true)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_blogpostid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""adx_blogid"" operator=""eq"" />
						</filter>
						<filter type=""or"">
							<condition attribute=""adx_published"" operator=""eq"" value=""true"" />						
						</filter>
					</entity>
				</fetch>");

			var andFilter = fetchXml.XPathSelectElement("//filter[@type='and']");

			if (andFilter == null)
			{
				throw new InvalidOperationException("Unable to select the blog post filter element.");
			}

			addFilterConditions(andFilter.AddFetchXmlFilterCondition);

			var blogIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_blogid']");

			if (blogIdAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_blogid filter"));
			}

			blogIdAttribute.SetAttributeValue("value", blogId.ToString());

			if (!published)
			{
				var publishedFilter = fetchXml.XPathSelectElement("//filter[@type='or']");

				if (publishedFilter == null)
				{
					throw new InvalidOperationException("Unable to select the blog post publishing filter element.");
				}

				publishedFilter.Remove();
			}

			var entity = serviceContext.RetrieveSingle(Fetch.Parse(fetchXml.ToString()));

			return (int)entity.GetAttributeValue<AliasedValue>("count").Value;
		}

		public static int FetchBlogPostCountForWebsite(this OrganizationServiceContext serviceContext, Guid websiteId, string tag)
		{
			if (string.IsNullOrWhiteSpace(tag))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "tag");
			}

			// If multi-language is enabled, only select blog posts of blogs that are language-agnostic or match the current language.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string conditionalLanguageFilter = contextLanguageInfo.IsCrmMultiLanguageEnabled
				? string.Format(@"<filter type=""or"">
						<condition attribute=""adx_websitelanguageid"" operator=""null"" />
						<condition attribute=""adx_websitelanguageid"" operator=""eq"" value=""{0}"" />
					</filter>", contextLanguageInfo.ContextLanguage.EntityReference.Id.ToString("D"))
				: string.Empty;

			var fetchXml = XDocument.Parse(string.Format(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_blogpostid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""adx_published"" operator=""eq"" value=""true"" />
						</filter>
						<link-entity name=""adx_blog"" from=""adx_blogid"" to=""adx_blogid"">
							<filter type=""and"">
								<condition attribute=""adx_websiteid"" operator=""eq"" />
								{0}
							</filter>
						</link-entity>
						<link-entity name=""adx_blogpost_tag"" from=""adx_blogpostid"" to=""adx_blogpostid"">
							<link-entity name=""adx_tag"" from=""adx_tagid"" to=""adx_tagid"">
								<filter type=""and"">
									<condition attribute=""adx_name"" operator=""eq"" />
								</filter>
							</link-entity>
						</link-entity>
					</entity>
				</fetch>", conditionalLanguageFilter));

			var websiteIdCondition = fetchXml.XPathSelectElement("//link-entity[@name='adx_blog']//condition[@attribute='adx_websiteid']");

			if (websiteIdCondition == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_websiteid filter"));
			}

			websiteIdCondition.SetAttributeValue("value", websiteId.ToString());

			var tagNameCondition = fetchXml.XPathSelectElement("//link-entity[@name='adx_tag']/filter/condition[@attribute='adx_name']");

			if (tagNameCondition == null)
			{
				throw new InvalidOperationException("Unable to select the tag name filter element.");
			}

			tagNameCondition.SetAttributeValue("value", tag);

			var entity = serviceContext.RetrieveSingle(Fetch.Parse(fetchXml.ToString()));

			return (int)entity.GetAttributeValue<AliasedValue>("count").Value;
		}

		public static int FetchBlogPostCountForWebsite(this OrganizationServiceContext serviceContext, Guid websiteId, Action<Action<string, string, string>> addFilterConditions)
		{
			// If multi-language is enabled, only select blog posts of blogs that are language-agnostic or match the current language.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string conditionalLanguageFilter = contextLanguageInfo.IsCrmMultiLanguageEnabled
				? string.Format(@"<filter type=""or"">
						<condition attribute=""adx_websitelanguageid"" operator=""null"" />
						<condition attribute=""adx_websitelanguageid"" operator=""eq"" value=""{0}"" />
					</filter>", contextLanguageInfo.ContextLanguage.EntityReference.Id.ToString("D"))
				: string.Empty;

			var fetchXml = XDocument.Parse(string.Format(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_blogpostid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""adx_published"" operator=""eq"" value=""true"" />
						</filter>
						<link-entity name=""adx_blog"" from=""adx_blogid"" to=""adx_blogid"">
							<filter type=""and"">
								<condition attribute=""adx_websiteid"" operator=""eq"" />
								{0}
							</filter>
						</link-entity>
					</entity>
				</fetch>", conditionalLanguageFilter));

			var andFilter = fetchXml.XPathSelectElement("//entity/filter[@type='and']");

			if (andFilter == null)
			{
				throw new InvalidOperationException("Unable to select the blog post filter element.");
			}

			addFilterConditions(andFilter.AddFetchXmlFilterCondition);

			var websiteIdCondition = fetchXml.XPathSelectElement("//link-entity[@name='adx_blog']//condition[@attribute='adx_websiteid']");

			if (websiteIdCondition == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_websiteid filter"));
			}

			websiteIdCondition.SetAttributeValue("value", websiteId.ToString());

			var entity = serviceContext.RetrieveSingle(Fetch.Parse(fetchXml.ToString()));

			return (int)entity.GetAttributeValue<AliasedValue>("count").Value;
		}

		public static IEnumerable<Tuple<int, int, int>> FetchBlogPostCountsGroupedByMonth(this OrganizationServiceContext serviceContext, Guid blogId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"" distinct=""false"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_blogpostid"" alias=""count"" aggregate=""count"" />
						<attribute name=""adx_date"" groupby=""true"" dategrouping=""month"" alias=""month"" />
						<attribute name=""adx_date"" groupby=""true"" dategrouping=""year"" alias=""year"" />
						<filter type=""and"">
							<condition attribute=""adx_blogid"" operator=""eq"" />
							<condition attribute=""adx_published"" operator=""eq"" value=""true"" />
						</filter>
					</entity>
				</fetch>");

			var websiteIdCondition = fetchXml.XPathSelectElement("//filter/condition[@attribute='adx_blogid']");

			if (websiteIdCondition == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_blogid filter condition"));
			}

			websiteIdCondition.SetAttributeValue("value", blogId.ToString());

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			return response.Entities.Select(e =>
			{
				var year = (int)e.GetAttributeValue<AliasedValue>("year").Value;
				var month = (int)e.GetAttributeValue<AliasedValue>("month").Value;
				var count = (int)e.GetAttributeValue<AliasedValue>("count").Value;

				return new Tuple<int, int, int>(year, month, count);
			});
		}

		public static IEnumerable<Tuple<int, int, int>> FetchBlogPostCountsGroupedByMonthInWebsite(this OrganizationServiceContext serviceContext, Guid websiteId)
		{
			// If multi-language is enabled, only select blog posts of blogs that are language-agnostic or match the current language.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string conditionalLanguageFilter = contextLanguageInfo.IsCrmMultiLanguageEnabled
				? string.Format(@"<filter type=""or"">
						<condition attribute=""adx_websitelanguageid"" operator=""null"" />
						<condition attribute=""adx_websitelanguageid"" operator=""eq"" value=""{0}"" />
					</filter>", contextLanguageInfo.ContextLanguage.EntityReference.Id.ToString("D"))
				: string.Empty;

			var fetchXml = XDocument.Parse(string.Format(@"
				<fetch mapping=""logical"" aggregate=""true"" distinct=""false"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_blogpostid"" alias=""count"" aggregate=""count"" />
						<attribute name=""adx_date"" groupby=""true"" dategrouping=""month"" alias=""month"" />
						<attribute name=""adx_date"" groupby=""true"" dategrouping=""year"" alias=""year"" />
						<filter type=""and"">
							<condition attribute=""adx_published"" operator=""eq"" value=""true"" />
						</filter>
						<link-entity name=""adx_blog"" from=""adx_blogid"" to=""adx_blogid"">
							<filter type=""and"">
								<condition attribute=""adx_websiteid"" operator=""eq"" />
								{0}
							</filter>
						</link-entity>
					</entity>
				</fetch>", conditionalLanguageFilter));

			var websiteIdCondition = fetchXml.XPathSelectElement("//link-entity[@name='adx_blog']/filter/condition[@attribute='adx_websiteid']");

			if (websiteIdCondition == null)
			{
				throw new InvalidOperationException("Unable to select the adx_websiteid filter condition element.");
			}

			websiteIdCondition.SetAttributeValue("value", websiteId.ToString());

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			return response.Entities.Select(e =>
			{
				var year = (int)e.GetAttributeValue<AliasedValue>("year").Value;
				var month = (int)e.GetAttributeValue<AliasedValue>("month").Value;
				var count = (int)e.GetAttributeValue<AliasedValue>("count").Value;

				return new Tuple<int, int, int>(year, month, count);
			});
		}

		public static IDictionary<Guid, Tuple<string, string, BlogCommentPolicy, string[], IRatingInfo>> FetchBlogPostExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> postIds, BlogCommentPolicy defaultCommentPolicy, Guid websiteId)
		{
			if (!postIds.Any())
			{
				return new Dictionary<Guid, Tuple<string, string, BlogCommentPolicy, string[], IRatingInfo>>();
			}

			var ids = postIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""adx_blogpost"">
						<attribute name=""adx_commentpolicy"" />
						<filter type=""and"">
						</filter>
						<link-entity name=""adx_blog"" from=""adx_blogid"" to=""adx_blogid"" alias=""blog"">
							<attribute name=""adx_commentpolicy"" />
						</link-entity>
						<link-entity link-type=""outer"" name=""contact"" from=""contactid"" to=""adx_authorid"" alias=""author"">
							<attribute name=""contactid"" />
							<attribute name=""fullname"" />
							<attribute name=""firstname"" />
							<attribute name=""lastname"" />
							<attribute name=""emailaddress1"" />
						</link-entity>
						<link-entity link-type=""outer"" name=""adx_blogpost_tag"" from=""adx_blogpostid"" to=""adx_blogpostid"">
							<link-entity link-type=""outer"" name=""adx_tag"" from=""adx_tagid"" to=""adx_tagid"" alias=""tag"">
								<attribute name=""adx_name"" />
								<filter type=""and"">
									<condition attribute=""adx_websiteid"" operator=""eq"" />
								</filter>
							</link-entity>
						</link-entity>
					</entity>
				</fetch>");

			var postFilter = fetchXml.XPathSelectElement("//entity[@name='adx_blogpost']/filter");

			if (postFilter == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_blogpostid filter condition"));
			}

			postFilter.AddFetchXmlFilterInCondition("adx_blogpostid", ids.Select(id => id.ToString()));

			var websiteConditions = fetchXml.XPathSelectElements("//filter/condition[@attribute='adx_websiteid']");

			foreach (var websiteCondition in websiteConditions)
			{
				websiteCondition.SetAttributeValue("value", websiteId.ToString());
			}

			var fetch = Fetch.Parse(fetchXml.ToString());
			var contactLink = fetch.Entity.Links.FirstOrDefault(l => l.Name.Equals("contact"));
			if (contactLink != null)
			{
				contactLink.IsUnique = true;
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(fetch);

			var aggregateFilter = fetchXml.Descendants("filter").First();

			aggregateFilter.AddFetchXmlFilterInCondition("adx_blogpostid", ids.Select(id => id.ToString()));
			EntityCollection aggregateResponse = null;

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				XDocument aggregateFetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""feedback"">
						<attribute name=""regardingobjectid"" alias=""ratingcount"" aggregate=""countcolumn""/>
						<attribute name=""rating"" alias=""ratingsum"" aggregate=""sum"" />
						<attribute name=""rating"" alias=""value"" groupby=""true"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<condition attribute=""rating"" operator=""not-null"" />
						</filter>
						<link-entity name=""adx_blogpost"" from=""adx_blogpostid"" to=""regardingobjectid"">
							<attribute name=""adx_blogpostid"" alias=""postid"" groupby=""true"" />
							<filter type=""and"" />
						</link-entity>
					</entity>
				</fetch>");

				aggregateResponse = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(aggregateFetchXml.ToString()));
			}
			return ids.ToDictionary(id => id, id =>
			{
				var entities = response.Entities.Where(e => e.Id == id).ToArray();

				if (!entities.Any())
				{
					return new Tuple<string, string, BlogCommentPolicy, string[], IRatingInfo>(null, null, defaultCommentPolicy, new string[] { }, null);
				}

				var entity = entities.First();

				var authorName = Localization.LocalizeFullName(entity.GetAttributeAliasedValue<string>("author.firstname"), entity.GetAttributeAliasedValue<string>("author.lastname"));
				var authorEmail = entity.GetAttributeAliasedValue<string>("author.emailaddress1");

				object postCommentPolicyAttributeValue;
				var postCommentPolicy = entity.Attributes.TryGetValue("adx_commentpolicy", out postCommentPolicyAttributeValue) && (postCommentPolicyAttributeValue is OptionSetValue)
					? (BlogPostCommentPolicy)Enum.ToObject(typeof(BlogPostCommentPolicy), ((OptionSetValue)postCommentPolicyAttributeValue).Value)
					: BlogPostCommentPolicy.Inherit;

				var blogCommentPolicyOption = entity.GetAttributeAliasedValue<OptionSetValue>("blog.adx_commentpolicy");
				var blogCommentPolicy = blogCommentPolicyOption == null
					? defaultCommentPolicy
					: (BlogCommentPolicy)Enum.ToObject(typeof(BlogCommentPolicy), blogCommentPolicyOption.Value);

				var commentPolicy = postCommentPolicy == BlogPostCommentPolicy.Inherit
					? blogCommentPolicy
					: (BlogCommentPolicy)Enum.ToObject(typeof(BlogCommentPolicy), (int)postCommentPolicy);

				var tags = entities
					.Select(e => e.GetAttributeAliasedValue<string>("tag.adx_name"))
					.Where(tagName => !string.IsNullOrWhiteSpace(tagName))
					.ToArray();

				int ratingCount = 0;
				int ratingSum = 0;
				double averageRating = 0;
				int yesCount = 0;
				int noCount = 0;

				if (aggregateResponse != null)
				{
					var aggregateResults = aggregateResponse.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("postid") == id);

					var aggregateYesResult = aggregateResponse.Entities
						.Where(e => e.GetAttributeAliasedValue<Guid?>("postid") == id)
						.FirstOrDefault(e => e.GetAttributeAliasedValue<int?>("value") == 1);

					var aggregateNoResult = aggregateResponse.Entities
						.Where(e => e.GetAttributeAliasedValue<Guid?>("postid") == id)
						.FirstOrDefault(e => e.GetAttributeAliasedValue<int?>("value") == 0);

					yesCount = (aggregateYesResult != null) ? aggregateYesResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0 : 0;

					noCount = (aggregateNoResult != null) ? aggregateNoResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0 : 0;
					
					foreach (var aggregateResult in aggregateResults)
					{
						ratingCount = ratingCount + aggregateResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0;
						ratingSum = ratingSum + aggregateResult.GetAttributeAliasedValue<int?>("ratingsum") ?? 0;
					}

					if (ratingCount == 0)
					{
						averageRating = 0;
					}
					else
					{
						averageRating = ratingSum / (double)ratingCount;
					}
				}

				var ratingInfo = new RatingInfo(yesCount, noCount, averageRating, ratingCount, ratingSum);

				return new Tuple<string, string, BlogCommentPolicy, string[], IRatingInfo>(authorName, authorEmail, commentPolicy, tags, ratingInfo);
			});
		}

		public static IDictionary<Guid, Tuple<string, string, string, IRatingInfo>> FetchBlogPostCommentExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> commentIds)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new Dictionary<Guid, Tuple<string, string, string, IRatingInfo>>();
			}
			if (!commentIds.Any())
			{
				return new Dictionary<Guid, Tuple<string, string, string, IRatingInfo>>();
			}

			var ids = commentIds.ToArray();

			XDocument fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""feedback"">
						<filter type=""and"">
						</filter>
						<link-entity name=""contact"" from=""contactid"" to=""createdbycontact"" alias=""author"">
							<attribute name=""contactid"" />
							<attribute name=""fullname"" />
							<attribute name=""firstname"" />
							<attribute name=""lastname"" />
							<attribute name=""emailaddress1"" />
							<attribute name=""websiteurl"" />
						</link-entity>
					</entity>
				</fetch>");
			
			var filter = fetchXml.Descendants("filter").First();

			filter.AddFetchXmlFilterInCondition("feedbackid", ids.Select(id => id.ToString()));

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			XDocument aggregateFetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""feedback"">
						<attribute name=""regardingobjectid"" alias=""ratingcount"" aggregate=""countcolumn""/>
						<attribute name=""rating"" alias=""ratingsum"" aggregate=""sum"" />
						<attribute name=""rating"" alias=""value"" groupby=""true"" />
						<link-entity name=""feedback"" from=""feedbackid"" to=""regardingobjectid"">
							<attribute name=""feedbackid"" alias=""commentid"" groupby=""true"" />
							<filter type=""and"" />
						</link-entity>
					</entity>
				</fetch>");

			var aggregateFilter = fetchXml.Descendants("filter").First();

			aggregateFilter.AddFetchXmlFilterInCondition("feedbackid", ids.Select(id => id.ToString()));

			var aggregateResponse = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(aggregateFetchXml.ToString()));

			return ids.ToDictionary(id => id, id =>
			{
				var entity = response.Entities.FirstOrDefault(e => e.Id == id);

				if (entity == null)
				{
					return new Tuple<string, string, string, IRatingInfo>(null, null, null, null);
				}

				var authorName = Localization.LocalizeFullName(entity.GetAttributeAliasedValue<string>("author.firstname"), entity.GetAttributeAliasedValue<string>("author.lastname"));
				var authorUrl = entity.GetAttributeAliasedValue<string>("author.websiteurl");
				var authorEmail = entity.GetAttributeAliasedValue<string>("author.emailaddress1");

				var aggregateResults = aggregateResponse.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("commentid") == id);

				var aggregateYesResult = aggregateResponse.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("commentid") == id)
					.FirstOrDefault(e => e.GetAttributeAliasedValue<int?>("value") == 1);

				var aggregateNoResult = aggregateResponse.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("commentid") == id)
					.FirstOrDefault(e => e.GetAttributeAliasedValue<int?>("value") == 0);

				var yesCount = (aggregateYesResult != null) ? aggregateYesResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0 : 0;
				var noCount = (aggregateNoResult != null) ? aggregateNoResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0 : 0;

				var ratingCount = 0;
				var ratingSum = 0;

				foreach (var aggregateResult in aggregateResults)
				{
					ratingCount = ratingCount + aggregateResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0;
					ratingSum = ratingSum + aggregateResult.GetAttributeAliasedValue<int?>("ratingsum") ?? 0;
				}

				double averageRating;

				if (ratingCount == 0)
				{
					averageRating = 0;
				}
				else
				{
					averageRating = ratingSum / (double)ratingCount;
				}

				var ratingInfo = new RatingInfo(yesCount, noCount, averageRating, ratingCount, ratingSum);

				return new Tuple<string, string, string, IRatingInfo>(authorName, authorUrl, authorEmail, ratingInfo);
			});
		}

		public static IEnumerable<Tuple<string, int>> FetchBlogPostTagCounts(this OrganizationServiceContext serviceContext, Guid blogId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"" distinct=""false"">
					<entity name=""adx_blogpost_tag"">
						<attribute name=""adx_blogpostid"" alias=""count"" aggregate=""count"" />
						<link-entity name=""adx_tag"" from=""adx_tagid"" to=""adx_tagid"">
							<attribute name=""adx_name"" groupby=""true"" alias=""tag"" />
						</link-entity>
						<link-entity name=""adx_blogpost"" from=""adx_blogpostid"" to=""adx_blogpostid"">
							<filter type=""and"">
								<condition attribute=""adx_blogid"" operator=""eq"" />
								<condition attribute=""adx_published"" operator=""eq"" value=""true"" />
							</filter>
						</link-entity>
					</entity>
				</fetch>");

			var websiteIdCondition = fetchXml.XPathSelectElement("//link-entity[@name='adx_blogpost']/filter/condition[@attribute='adx_blogid']");

			if (websiteIdCondition == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_blogid filter condition"));
			}

			websiteIdCondition.SetAttributeValue("value", blogId.ToString());

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			return response.Entities.Select(e =>
			{
				var tag = (string)e.GetAttributeValue<AliasedValue>("tag").Value;
				var count = (int)e.GetAttributeValue<AliasedValue>("count").Value;

				return new Tuple<string, int>(tag, count);
			}).ToArray();
		}

		public static IEnumerable<Tuple<string, int>> FetchBlogPostTagCountsInWebsite(this OrganizationServiceContext serviceContext, Guid websiteId)
		{
			// If multi-language is enabled, only select blog posts of blogs that are language-agnostic or match the current language.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string conditionalLanguageFilter = contextLanguageInfo.IsCrmMultiLanguageEnabled
				? string.Format(@"<filter type=""or"">
						<condition attribute=""adx_websitelanguageid"" operator=""null"" />
						<condition attribute=""adx_websitelanguageid"" operator=""eq"" value=""{0}"" />
					</filter>", contextLanguageInfo.ContextLanguage.EntityReference.Id.ToString("D"))
				: string.Empty;

			var fetchXml = XDocument.Parse(string.Format(@"
				<fetch mapping=""logical"" aggregate=""true"" distinct=""false"">
					<entity name=""adx_blogpost_tag"">
						<attribute name=""adx_blogpostid"" alias=""count"" aggregate=""count"" />
						<link-entity name=""adx_tag"" from=""adx_tagid"" to=""adx_tagid"">
							<attribute name=""adx_name"" groupby=""true"" alias=""tag"" />
						</link-entity>
						<link-entity name=""adx_blogpost"" from=""adx_blogpostid"" to=""adx_blogpostid"">
							<filter type=""and"">
								<condition attribute=""adx_published"" operator=""eq"" value=""true"" />
							</filter>
							<link-entity name=""adx_blog"" from=""adx_blogid"" to=""adx_blogid"">
								<filter type=""and"">
									<condition attribute=""adx_websiteid"" operator=""eq"" />
									{0}
								</filter>
							</link-entity>
						</link-entity>
					</entity>
				</fetch>", conditionalLanguageFilter));

			var websiteIdCondition = fetchXml.XPathSelectElement("//condition[@attribute='adx_websiteid']");

			if (websiteIdCondition == null)
			{
				throw new InvalidOperationException("Unable to select the adx_websiteid filter condition element.");
			}

			websiteIdCondition.SetAttributeValue("value", websiteId.ToString());

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			return response.Entities.Select(e =>
			{
				var tag = (string)e.GetAttributeValue<AliasedValue>("tag").Value;
				var count = (int)e.GetAttributeValue<AliasedValue>("count").Value;

				return new Tuple<string, int>(tag, count);
			}).ToArray();
		}
	}
}
