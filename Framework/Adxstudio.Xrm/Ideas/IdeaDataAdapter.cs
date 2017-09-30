/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Adxstudio.Xrm.Text;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Feedback;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea such as comments and votes.
	/// </summary>
	public class IdeaDataAdapter : IIdeaDataAdapter
	{
		private bool? _hasCommentModerationPermission;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">The idea to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IdeaDataAdapter(EntityReference idea, IDataAdapterDependencies dependencies)
		{
			idea.ThrowOnNull("idea");
			idea.AssertLogicalName("adx_idea");
			dependencies.ThrowOnNull("dependencies");

			Idea = idea;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">The idea to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IdeaDataAdapter(Entity idea, IDataAdapterDependencies dependencies) : this(idea.ToEntityReference(), dependencies) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">The idea to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IdeaDataAdapter(IIdea idea, IDataAdapterDependencies dependencies) : this(idea.Entity, dependencies) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">The idea to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaDataAdapter(EntityReference idea, string portalName = null) : this(idea, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">The idea to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaDataAdapter(Entity idea, string portalName = null) : this(idea.ToEntityReference(), portalName) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">The idea to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaDataAdapter(IIdea idea, string portalName = null) : this(idea.Entity, portalName) { }

		protected EntityReference Idea { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		/// <summary>
		/// Gets or sets whether or not comments should be in chronological order (default false [reverse chronological]).
		/// </summary>
		public bool? ChronologicalComments { get; set; }

		/// <summary>
		/// Post a comment for the idea this adapter applies to.
		/// </summary>
		/// <param name="content">The comment copy.</param>
		/// <param name="authorName">The name of the author for this comment (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for this comment (ignored if user is authenticated).</param>
		public virtual void CreateComment(string content, string authorName = null, string authorEmail = null)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return;
			}
			content.ThrowOnNullOrWhitespace("content");

			var title = StringHelper.GetCommentTitleFromContent(content);
			title.ThrowOnNullOrWhitespace("title");

			var httpContext = Dependencies.GetHttpContext();
			var author = Dependencies.GetPortalUser();

			if (!httpContext.Request.IsAuthenticated || author == null)
			{
				authorName.ThrowOnNullOrWhitespace("authorName", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "idea comment"));
				authorEmail.ThrowOnNullOrWhitespace("authorEmail", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "idea comment"));
			}

			var context = Dependencies.GetServiceContext();

			var idea = Select();

			if (!idea.CurrentUserCanComment)
			{
				throw new InvalidOperationException("An idea comment can't be created with the current idea comment policy.");
			}

			var comment = new Entity("feedback");

			comment["title"] = title;
			comment["comments"] = content;
			comment["regardingobjectid"] = Idea;
			comment["createdon"] = DateTime.UtcNow;
			comment["adx_approved"] = idea.CommentPolicy != IdeaForumCommentPolicy.Moderated;
			comment["adx_createdbycontact"] = authorName;
			comment["adx_contactemail"] = authorEmail;
			comment["source"] = new OptionSetValue((int)FeedbackSource.Portal);

			if (author != null && author.LogicalName == "contact")
			{
				comment[FeedbackMetadataAttributes.UserIdAttributeName] = author;
			}
			else if (context != null)
			{
				comment[FeedbackMetadataAttributes.VisitorAttributeName] = httpContext.Profile.UserName;
			}
			context.AddObject(comment);
			context.SaveChanges();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Idea, HttpContext.Current, "create_idea_comment", 1, comment.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Cast a vote/votes for the idea this adapter applies to.
		/// </summary>
		/// <param name="voteValue">The number of votes to cast.</param>
		/// <param name="voterName">The name of the voter (ignored if user is authenticated).</param>
		/// <param name="voterEmail">The email of the voter (ignored if user is authenticated).</param>
		public virtual void CreateVote(int voteValue, string voterName = null, string voterEmail = null)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return;
			}
			var httpContext = Dependencies.GetHttpContext();
			var voter = Dependencies.GetPortalUser();

			if (!httpContext.Request.IsAuthenticated || voter == null)
			{
				voterName.ThrowOnNullOrWhitespace("voterName", ResourceManager.GetString("Anonymous_Voting_Null_Exception"));
				voterEmail.ThrowOnNullOrWhitespace("voterEmail", ResourceManager.GetString("Anonymous_Voting_Null_Exception"));
			}

			var context = Dependencies.GetServiceContext();

			var idea = Select();

			if (!idea.CurrentUserCanVote(voteValue))
			{
				throw new InvalidOperationException("An idea vote can't be created with the current voting limits and policy.");
			}
			
			var ideaMetadata = GetIdeaEntityMetadata();

			var displayName = ideaMetadata != null ? ideaMetadata.DisplayName.UserLocalizedLabel.Label : string.Empty;

			var vote = new Entity("feedback");
			vote["title"] = ResourceManager.GetString("Feedback_Default_Title").FormatWith(displayName, idea.Title);
			vote["regardingobjectid"] = Idea;
			vote["modifiedon"] = DateTime.UtcNow;
			vote["rating"] = voteValue;
			vote["adx_createdbycontact"] = voterName;
			vote["adx_contactemail"] = voterEmail;
			vote["source"] = new OptionSetValue((int)FeedbackSource.Portal);

			var portalUser = Dependencies.GetPortalUser();

			if (portalUser != null && portalUser.LogicalName == "contact")
			{
				vote[FeedbackMetadataAttributes.UserIdAttributeName] = portalUser;
			}
			else if (context != null)
			{
				vote[FeedbackMetadataAttributes.VisitorAttributeName] = httpContext.Profile.UserName;
			}

			var minRating = GetMinRating(idea.VotingType);
			var maxRating = GetMaxRating(idea.VotingType, context, Idea.Id);

			if (minRating != null)
			{
				vote["minrating"] = minRating;
			}

			if (maxRating != null)
			{
				vote["maxrating"] = maxRating;
			}

			vote["adx_approved"] = idea.CommentPolicy != IdeaForumCommentPolicy.Moderated;

			if (voter != null)
			{
				vote["createdbycontact"] = voter;
			}
			else
			{
				vote["createdbyname"] = voterName;
			}

			context.AddObject(vote);
			context.SaveChanges();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Idea, HttpContext.Current, "vote_idea", 1, idea.Entity.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Returns the <see cref="IIdea"/> that this adapter applies to.
		/// </summary>
		public virtual IIdea Select()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var idea = GetIdeaEntity(serviceContext);

			return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(new[] { idea }).FirstOrDefault();
		}

		/// <summary>
		/// Returns comments that have been posted for the idea this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		public virtual IEnumerable<IComment> SelectComments(int startRowIndex = 0, int maximumRows = -1)
		{
			var comments = new List<Comment>();
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) || maximumRows == 0)
			{
				return comments;
			}
			var includeUnapprovedComments = TryAssertCommentModerationPermission(Dependencies.GetServiceContext());
			var query =
				Cms.OrganizationServiceContextExtensions.SelectCommentsByPage(
					Cms.OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows), Idea.Id,
					includeUnapprovedComments, ChronologicalComments);
			var commentsEntitiesResult = Dependencies.GetServiceContext().RetrieveMultiple(query);
			comments.AddRange(
				commentsEntitiesResult.Entities.Select(
					commentEntity =>
						new Comment(commentEntity,
							new Lazy<ApplicationPath>(() => Dependencies.GetEditPath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<ApplicationPath>(() => Dependencies.GetDeletePath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<bool>(() => includeUnapprovedComments, LazyThreadSafetyMode.None), ratingEnabled: true)));
			return comments;

		}
	

		/// <summary>
		/// Returns the number of comments that have been posted for the idea this adapter applies to.
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
				addCondition("regardingobjectid", "eq", Idea.Id.ToString());
				addCondition("statecode", "eq", "0");

				if (!includeUnapprovedComments)
				{
					addCondition("adx_approved", "eq", "true");
				}
			},
			null,
			addBinaryCondition => addBinaryCondition("comments", "not-null"));
		}

		protected virtual bool TryAssertCommentModerationPermission(OrganizationServiceContext serviceContext)
		{
			if (_hasCommentModerationPermission.HasValue)
			{
				return _hasCommentModerationPermission.Value;
			}

			var security = Dependencies.GetSecurityProvider();

			_hasCommentModerationPermission = security.TryAssert(serviceContext, GetIdeaEntity(serviceContext), CrmEntityRight.Change);

			return _hasCommentModerationPermission.Value;
		}

		private Entity GetIdeaEntity(OrganizationServiceContext serviceContext)
		{
			var idea = serviceContext.RetrieveSingle("adx_idea", "adx_ideaid", this.Idea.Id, FetchAttribute.All);

			if (idea == null)
			{
				throw new InvalidOperationException(ResourceManager.GetString("ADX_Idea_NotFound").FormatWith(Idea.Id));
			}

			return idea;
		}

		private static int? GetVotesPerIdea(OrganizationServiceContext context, Guid ideaId)
		{
			if (context == null)
				return null;

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_ideaforum", new[] { "adx_votesperidea" })
						{
							Links = new[]
							{
								new Link
								{
									Name = "adx_idea",
									ToAttribute = "adx_ideaforumid",
									FromAttribute = "adx_ideaforumid",
									Filters = new[]
									{
										new Filter
										{
											Conditions = new[] { new Condition("adx_ideaid", ConditionOperator.Equal, ideaId) }
										}
									}
								}
							}
						}
			};

			var ideaForum = context.RetrieveSingle(fetch);

			return ideaForum == null ? null : ideaForum.GetAttributeValue<int?>("adx_votesperidea");
		}

		private static int? GetMaxRating(IdeaForumVotingType type, OrganizationServiceContext context, Guid ideaId)
		{
			if (type == IdeaForumVotingType.UpOrDown)
			{
				return 1;
			}

			if (type == IdeaForumVotingType.UpOnly)
			{
				return GetVotesPerIdea(context, ideaId) ?? 1;
			}

			return null;
		}

		private static int? GetMinRating(IdeaForumVotingType type)
		{
			if (type == IdeaForumVotingType.UpOnly)
			{
				return 0;
			}

			if (type == IdeaForumVotingType.UpOrDown)
			{
				return -1;
			}

			return null;
		}

		/// <summary>
		/// Retrieve Idea EntityMetadata
		/// </summary>
		/// <returns>Idea EntityMetadata</returns>
		private EntityMetadata GetIdeaEntityMetadata()
		{
			var context = Dependencies.GetServiceContext();

			var metadataRequest = new RetrieveEntityRequest()
			{
				EntityFilters = EntityFilters.All,
				LogicalName = Idea.LogicalName,
				RetrieveAsIfPublished = false
			};

			return ((RetrieveEntityResponse)context.Execute(metadataRequest)).EntityMetadata;
		}
	}
}
