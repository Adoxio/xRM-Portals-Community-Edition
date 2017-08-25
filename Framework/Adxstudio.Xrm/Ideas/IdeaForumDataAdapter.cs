/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;

	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;

	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea Forum such as ideas.
	/// </summary>
	/// <remarks>Ideas are returned ordered by their vote sum (positive votes minus negative votes, highest first).</remarks>
	public class IdeaForumDataAdapter : IIdeaForumDataAdapter
	{
		private bool? _hasIdeaPreviewPermission;

		/// <summary>
		/// Returns the attribute to be used in Data Selection process
		/// </summary>
		protected virtual string OrderAttribute
		{
			get { return "adx_votessum"; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ideaForum">The idea forum to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IdeaForumDataAdapter(EntityReference ideaForum, IDataAdapterDependencies dependencies)
		{
			ideaForum.ThrowOnNull("ideaForum");
			ideaForum.AssertLogicalName("adx_ideaforum");
			dependencies.ThrowOnNull("dependencies");

			IdeaForum = ideaForum;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ideaForum">The idea forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaForumDataAdapter(Entity ideaForum, string portalName = null) : this(ideaForum.ToEntityReference(), new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference IdeaForum { get; private set; }

		public DateTime? MaxDate { get; set; }

		public DateTime? MinDate { get; set; }

		public int? Status { get; set; }

		/// <summary>
		/// The Expiration Duration in which to keep the queries in the cache
		/// </summary>
		private TimeSpan? ExpirationDuration
		{
			get
			{
				return this is IRollupFreeIdeaForumDataAdapter
					? (TimeSpan?)null
					: TimeSpan.FromHours(1);
			}
		}

		/// <summary>
		/// Submit an idea to the idea forum this aadapter applies to.
		/// </summary>
		/// <param name="title">The title of the idea.</param>
		/// <param name="copy">The copy of the idea.</param>
		/// <param name="authorName">The name of the author for the idea (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for the idea (ignored if user is authenticated).</param>
		public virtual void CreateIdea(string title, string copy, string authorName = null, string authorEmail = null)
		{
			title.ThrowOnNullOrWhitespace("title");

			var httpContext = Dependencies.GetHttpContext();
			var author = Dependencies.GetPortalUser();

			if (!httpContext.Request.IsAuthenticated || author == null)
			{
				authorName.ThrowOnNullOrWhitespace("authorName", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "idea"));
				authorEmail.ThrowOnNullOrWhitespace("authorEmail", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "idea"));
			}

			var context = Dependencies.GetServiceContext();

			var ideaForum = Select();

			if (ideaForum == null)
			{
				throw new InvalidOperationException(string.Format("Can't find {0} having ID {1}.", "adx_ideaforum", IdeaForum.Id));
			}
			else if (!ideaForum.CurrentUserCanSubmitIdeas)
			{
				throw new InvalidOperationException(string.Format("The current user can't create an {0} with the current {0} submission policy.", "Idea"));
			}

			var username = httpContext.Request.IsAuthenticated
				? httpContext.User.Identity.Name
				: httpContext.Request.AnonymousID;

			var idea = new Entity("adx_idea");

			idea["adx_name"] = title;
			idea["adx_copy"] = copy;
			idea["adx_ideaforumid"] = IdeaForum;
			idea["adx_date"] = DateTime.UtcNow;
			idea["adx_partialurl"] = GetDefaultIdeaPartialUrl(title);
			idea["adx_createdbyusername"] = username;
			// idea["adx_createdbyipaddress"] = httpContext.Request.UserHostAddress;
			idea["adx_approved"] = ideaForum.IdeaSubmissionPolicy != IdeaForumIdeaSubmissionPolicy.Moderated;

			if (author != null)
			{
				idea["adx_authorid"] = author;
			}
			else
			{
				idea["adx_authorname"] = authorName;
				idea["adx_authoremail"] = authorEmail;
			}

			context.AddObject(idea);
			context.SaveChanges();

			var ideaDataAdapter = new IdeaDataAdapter(idea);
			if (ideaDataAdapter.Select().CurrentUserCanVote())
				ideaDataAdapter.CreateVote(1, authorName, authorEmail);

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Idea, HttpContext.Current, "create_idea", 1, idea.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Returns the <see cref="IIdeaForum"/> that this adapter applies to.
		/// If idea forum cannot be found, or not available in current language, then null will be returned.
		/// </summary>
		public virtual IIdeaForum Select()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_ideaforum")
				{
					Filters = new List<Filter>
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_ideaforumid", ConditionOperator.Equal, this.IdeaForum.Id),
							}
						}
					}
				}
			};

			var languageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				fetch.Entity.Filters.Add(
					new Filter
					{
						Type = LogicalOperator.Or,
						Conditions = new[]
						{
							new Condition("adx_websitelanguageid", ConditionOperator.Null),
							new Condition("adx_websitelanguageid", ConditionOperator.Equal, languageInfo.ContextLanguage.EntityReference.Id)
						}
					});
			}

			var ideaForumEntity = serviceContext.RetrieveSingle("adx_ideaforum", FetchAttribute.All, new Condition("adx_ideaforumid", ConditionOperator.Equal, this.IdeaForum.Id));

			if (ideaForumEntity == null)
			{
				return null;
			}

			var ideaForum = new IdeaForumFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(new[] { ideaForumEntity }).FirstOrDefault();

			// Gets metadata for Status Reason attribute
			var ideaStatusMetadata = GetIdeaAttributeMetadata(serviceContext, "statuscode");
			if (ideaStatusMetadata != null)
			{
				ideaForum.IdeaStatusOptionSetMetadata = ideaStatusMetadata.OptionSet;
			}

			return ideaForum;
		}

		/// <summary>
		/// Returns ideas that have been submitted to the idea forum this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		public virtual IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIdea[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedIdeas = TryAssertIdeaPreviewPermission(serviceContext);

			var filter = new Filter
			{
				Conditions = new List<Condition>
				{
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("adx_ideaforumid", ConditionOperator.Equal, IdeaForum.Id)
				}
			};

			var pageInfo = Cms.OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows);
			var orders = new List<Order>
			{
				new Order(this.OrderAttribute, OrderType.Descending),
			};

			if (this.OrderAttribute != "adx_date")
			{
				orders.Add(new Order("adx_date", OrderType.Descending));
			}

			var fetch = new Fetch()
			{
				Entity = new FetchEntity
				{
					Name = "adx_idea",
					Attributes = FetchAttribute.All,
					Orders = orders,
					Filters = new List<Filter>
					{
						filter
					},
				},
				PageNumber = pageInfo.PageNumber,
				PageSize = pageInfo.Count,
			};

			if (MaxDate.HasValue)
			{
				filter.Conditions.Add(new Condition("adx_date", ConditionOperator.LessThan, MaxDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture)));
			}

			if (MinDate.HasValue)
			{
				filter.Conditions.Add(new Condition("adx_date", ConditionOperator.GreaterThan, MinDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture)));
			}

			if (!includeUnapprovedIdeas)
			{
				filter.Conditions.Add(new Condition("adx_approved", ConditionOperator.Equal, "true"));
			}

			if (Status.HasValue)
			{
				filter.Conditions.Add(new Condition("statuscode", ConditionOperator.Equal, (int)Status.Value));
			}

			var collection = serviceContext.RetrieveMultiple(fetch, expiration: this.ExpirationDuration);

			var query = collection.Entities.AsEnumerable();

			if (!query.Any())
			{
				return new IIdea[] { };
			}

			return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(query);
		}

		/// <summary>
		/// Returns the number of ideas that have been submitted to the idea forum this adapter applies to.
		/// </summary>
		/// <returns></returns>
		public virtual int SelectIdeaCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedIdeas = TryAssertIdeaPreviewPermission(serviceContext);

			return serviceContext.FetchCount("adx_idea", "adx_ideaid", addCondition =>
			{
				addCondition("adx_ideaforumid", "eq", IdeaForum.Id.ToString());
				addCondition("statecode", "eq", "0");

				if (!includeUnapprovedIdeas)
				{
					addCondition("adx_approved", "eq", "true");
				}

				if (Status.HasValue)
				{
					addCondition("statuscode", "eq", "{0}".FormatWith((int)Status.Value));
				}

				if (MaxDate.HasValue)
				{
					addCondition("adx_date", "le", MaxDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
				}

				if (MinDate.HasValue)
				{
					addCondition("adx_date", "ge", MinDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
				}
			});
		}

		protected virtual bool TryAssertIdeaPreviewPermission(OrganizationServiceContext serviceContext)
		{
			if (_hasIdeaPreviewPermission.HasValue)
			{
				return _hasIdeaPreviewPermission.Value;
			}

			var ideaForum = serviceContext.RetrieveSingle(
				"adx_ideaforum",
				"adx_ideaforumid",
				this.IdeaForum.Id,
				FetchAttribute.All);

			if (ideaForum == null)
			{
				throw new InvalidOperationException(string.Format("Can't find {0} having ID {1}.", "adx_ideaforum", IdeaForum.Id));
			}

			var security = Dependencies.GetSecurityProvider();

			_hasIdeaPreviewPermission = security.TryAssert(serviceContext, ideaForum, CrmEntityRight.Change);

			return _hasIdeaPreviewPermission.Value;
		}

		private static string GetDefaultIdeaPartialUrl(string title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "title");
			}

			string titleSlug;

			try
			{
				// Encoding the title as Cyrillic, and then back to ASCII, converts accented characters to their
				// unaccented version. We'll try/catch this, since it depends on the underlying platform whether
				// the Cyrillic code page is available.
				titleSlug = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(title)).ToLowerInvariant();
			}
			catch
			{
				titleSlug = title.ToLowerInvariant();
			}

			// Strip all disallowed characters.
			titleSlug = Regex.Replace(titleSlug, @"[^a-z0-9\s-]", string.Empty);

			// Convert all runs of multiple spaces to a single space.
			titleSlug = Regex.Replace(titleSlug, @"\s+", " ").Trim();

			// Cap the length of the title slug.
			titleSlug = titleSlug.Substring(0, titleSlug.Length <= 50 ? titleSlug.Length : 50).Trim();

			// Replace all spaces with hyphens.
			titleSlug = Regex.Replace(titleSlug, @"\s", "-").Trim('-');

			return titleSlug;
		}

		/// <summary>
		/// Retrieves metadata for Idea attribute.
		/// </summary>
		/// <param name="serviceContext">Instance of the Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.</param>
		/// <param name="attributeName">Logical Name of attribute.</param>
		/// <returns>Idea attribute metadata.</returns>
		internal static EnumAttributeMetadata GetIdeaAttributeMetadata(OrganizationServiceContext serviceContext, string attributeName)
		{
			if (serviceContext == null || string.IsNullOrEmpty(attributeName))
			{
				return null;
			}

			var request = new RetrieveAttributeRequest
			{
				EntityLogicalName = "adx_idea",
				LogicalName = attributeName,
				RetrieveAsIfPublished = true
			};

			RetrieveAttributeResponse response = (RetrieveAttributeResponse)serviceContext.Execute(request);

			return (EnumAttributeMetadata)response.AttributeMetadata;
		}
	}
}
