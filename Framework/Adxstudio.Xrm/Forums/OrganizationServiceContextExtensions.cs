/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	public static class OrganizationServiceContextExtensions
	{
		public static IEnumerable<Entity> GetForums(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_communityforum").ToList();
		}

		public static IEnumerable<Entity> GetForumPosts(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_communityforumpost").ToList();
		}

		public static IEnumerable<Entity> GetForumThreads(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_communityforumthread").ToList();
		}

		public static IEnumerable<Entity> GetForumThreadTags(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_communityforumthreadtag").ToList();
		}

		public static IEnumerable<Entity> GetForumThreadTypes(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_forumthreadtype").ToList();
		}

		/// <summary>
		/// Marks forum post as an answer.
		/// </summary>
		public static void MarkPostAsAnswerAndSave(this OrganizationServiceContext context, Entity post, bool isAnswer)
		{
			post.AssertEntityName("adx_communityforumpost");
			post["adx_isanswer"] = isAnswer;
			context.UpdateObject(post);
			context.SaveChanges();
		}

		/// <summary>
		/// Increments the helpful vote count for forum post.
		/// </summary>
		public static void MarkPostAsHelpfulAndSave(this OrganizationServiceContext context, Entity post)
		{
			post.AssertEntityName("adx_communityforumpost");

			var count = post.GetAttributeValue<int?>("adx_helpfulvotecount");
			post["adx_helpfulvotecount"] = count + 1;
			context.UpdateObject(post);
			context.SaveChanges();
		}

		public static void ReportPostAbuseAndSave(this OrganizationServiceContext context, Entity post, string text)
		{
			post.AssertEntityName("adx_communityforumpost");

			context.AddNoteAndSave(post, string.Empty, text);
		}

		public static void AddThreadAlertAndSave(this OrganizationServiceContext context, Entity thread, Entity contact)
		{
			thread.AssertEntityName("adx_communityforumthread");
			contact.AssertEntityName("contact");

			var forumAlerts = thread.GetRelatedEntities(context, "adx_communityforumthread_communityforumaalert");

			var alert = (
				from e in forumAlerts
				where e.GetAttributeValue<EntityReference>("adx_subscriberid") == contact.ToEntityReference()
				select e).FirstOrDefault();

			if (alert == null)
			{
				alert = new Entity("adx_communityforumalert");
				alert["adx_subscriberid"] = contact.ToEntityReference();
                alert["adx_threadid"] = thread.ToEntityReference();

				context.AddObject(alert);
			}
			context.SaveChanges();
		}

		public static void RemoveThreadAlertAndSave(this OrganizationServiceContext context, Entity thread, Entity contact)
		{
			thread.AssertEntityName("adx_communityforumthread");
			contact.AssertEntityName("contact");

			var forumAlerts = thread.GetRelatedEntities(context, "adx_communityforumthread_communityforumaalert");

			var alert = (
				from e in forumAlerts
				where e.GetAttributeValue<EntityReference>("adx_subscriberid") == contact.ToEntityReference()
				select e).FirstOrDefault();

			if (alert != null)
			{
				context.DeleteObject(alert);
			}
			context.SaveChanges();
		}

		private static Entity GetForumThreadTagByName(this OrganizationServiceContext context, string tagName)
		{
			return context.CreateQuery("adx_communityforumthreadtag").ToList().Where(ftt => TagName.Equals(ftt.GetAttributeValue<string>("adx_name"), tagName)).FirstOrDefault();
		}

		/// <summary>
		/// Adds a Forum Thread Tag tag association by name to a Forum Thread.
		/// </summary>
		/// <param name="threadId">The ID of the Forum Thread whose tags will be affected.</param>
		/// <param name="tagName">
		/// The name of the tag to be associated with the thread (will be created if necessary).
		/// </param>
		/// <remarks>
		/// <para>
		/// This operation may call SaveChanges on this context--please ensure any queued
		/// changes are mananged accordingly.
		/// </para>
		/// </remarks>
		public static void AddTagToForumThreadAndSave(this OrganizationServiceContext context, Guid threadId, string tagName)
		{
			if (context.MergeOption == MergeOption.NoTracking)
			{
				throw new ArgumentException("The OrganizationServiceContext.MergeOption cannot be MergeOption.NoTracking.", "context");
			}

			if (string.IsNullOrEmpty(tagName))
			{
				throw new ArgumentException("Can't be null or empty.", "tagName");
			}

			if (threadId == Guid.Empty)
			{
				throw new ArgumentException("Argument must be a non-empty GUID.", "threadId");
			}

			var thread = context.CreateQuery("adx_communityforumthread").Single(e => e.GetAttributeValue<Guid>("adx_communityforumthreadid") == threadId);

			var tag = GetForumThreadTagByName(context, tagName);

			// If the tag doesn't exist, create it
			if (tag == null)
			{
				tag =  new Entity("adx_communityforumthreadtag");

				tag["adx_name"] = tagName;

				context.AddObject(tag);
				context.SaveChanges();
				context.ReAttach(thread);
				context.ReAttach(tag);
			}

			if (!thread.GetRelatedEntities(context, "adx_forumthreadtag_forumthread").Any(t => t.GetAttributeValue<Guid>("adx_communityforumthreadtagid") == tag.Id))
			{
				context.AddLink(thread, "adx_forumthreadtag_forumthread", tag);

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Removes a Forum Thread Tag tag association by name from a Forum Thread.
		/// </summary>
		/// <param name="threadId">The ID of the Forum Thread whose tags will be affected.</param>
		/// <param name="tagName">
		/// The name of the tag to be dis-associated with from the thread.
		/// </param>
		/// <remarks>
		/// <para>
		/// This operation may call SaveChanges on this context--please ensure any queued
		/// changes are mananged accordingly.
		/// </para>
		/// </remarks>
		public static void RemoveTagFromForumThreadAndSave(this OrganizationServiceContext context, Guid threadId, string tagName)
		{
			if (context.MergeOption == MergeOption.NoTracking)
			{
				throw new ArgumentException("The OrganizationServiceContext.MergeOption cannot be MergeOption.NoTracking.", "context");
			}

			if (string.IsNullOrEmpty(tagName))
			{
				throw new ArgumentException("Can't be null or empty.", "tagName");
			}

			if (threadId == Guid.Empty)
			{
				throw new ArgumentException("Argument must be a non-empty GUID.", "threadId");
			}

			var thread = context.CreateQuery("adx_communityforumthread").Single(e => e.GetAttributeValue<Guid>("adx_communityforumthreadid") == threadId);

			var tag = GetForumThreadTagByName(context, tagName);

			// If the tag doesn't exist, do nothing
			if (tag == null)
			{
				return;
			}

			context.DeleteLink(thread, "adx_forumthreadtag_forumthread", tag);
			context.SaveChanges();
		}

		#region Forum Moderation Methods

		/// <summary>
		/// Deletes a given Forum Thread.
		/// </summary>
		/// <param name="threadID">A unique identifier for the thread that will be removed.</param>
		public static void DeleteForumThread(this OrganizationServiceContext context, Guid threadID)
		{
			Entity thread;

			if (TryGetForumThreadFromID(context, threadID, out thread))
			{
				DeleteForumThread(context, thread);
			}
		}

		public static void DeleteForumThread(this OrganizationServiceContext context, Entity thread)
		{
			thread.AssertEntityName("adx_communityforumthread");

			context.DeleteObject(thread);

			context.SaveChanges();
		}

		/// <summary>
		/// Tries to get the thread for a given ID.
		/// </summary>
		/// <param name="threadID">A unique identifier for the thread.</param>
		/// <param name="thread">Entity output or null if retrieval was unsuccessful.</param>
		/// <returns>true, if the thread was successfully retrieved; otherwise, false.</returns>
		public static bool TryGetForumThreadFromID(this OrganizationServiceContext context, Guid threadID, out Entity thread)
		{
			thread = context.CreateQuery("adx_communityforumthread").Single(p => p.GetAttributeValue<Guid?>("adx_communityforumthreadid") == threadID);

			return thread != null;
		}

		/// <summary>
		/// Deletes a given Forum Post.
		/// </summary>
		/// <param name="postID">A unique identifier for the post that will be removed.</param>
		public static void DeleteForumPost(this OrganizationServiceContext context, Guid postID)
		{
			Entity post;

			if (TryGetForumPostFromID(context, postID, out post))
			{
				DeleteForumPost(context, post);
			}
		}

		public static void DeleteForumPost(this OrganizationServiceContext context, Entity post)
		{
			post.AssertEntityName("adx_communityforumpost");

			UpdateThreadOnPostDelete(context, post);

			context.ReAttach(post);

			context.DeleteObject(post);

			context.SaveChanges();
		}

		/// <summary>
		/// Tries to get the post for a given ID.
		/// </summary>
		/// <param name="postID">A unique identifier for the post.</param>
		/// <param name="post">Entity output or null if retrieval was unsuccessful.</param>
		/// <returns>true, if the post was successfully retrieved; otherwise, false.</returns>
		public static bool TryGetForumPostFromID(this OrganizationServiceContext context, Guid postID, out Entity post)
		{
			post = context.CreateQuery("adx_communityforumpost").Single(p => p.GetAttributeValue<Guid?>("adx_communityforumpostid") == postID);

			return post != null;
		}

		/// <summary>
		/// Updates the post's parent thread. If the the parent thread's Last Post ID or First Post ID match
		/// the id of the current post being deleted, then those ID's need to be updated.  This method will also
		/// decrement the thread's post count.
		/// </summary>
		/// <param name="post"> entity.</param>
		private static void UpdateThreadOnPostDelete(this OrganizationServiceContext context, Entity post)
		{
			post.AssertEntityName("adx_communityforumpost");

			var parentThread = post.GetRelatedEntity(context, "adx_communityforumthread_communityforumpost");

			if (parentThread == null) throw new NullReferenceException("Error retrieving parent Forum Thread");

			var currentLastPostId = parentThread.GetAttributeValue<EntityReference>("adx_lastpostid") == null ? Guid.Empty : parentThread.GetAttributeValue<EntityReference>("adx_lastpostid").Id;

			var currentFirstPostId = parentThread.GetAttributeValue<EntityReference>("adx_firstpostid") == null ? Guid.Empty : parentThread.GetAttributeValue<EntityReference>("adx_firstpostid").Id;

			var currentPostId = post.GetAttributeValue<Guid>("adx_communityforumpostid");

			if (currentPostId == currentLastPostId)
			{
				var lastPost = GetForumPosts(context).Where(p => p.GetAttributeValue<EntityReference>("adx_forumthreadid") != null && p.GetAttributeValue<EntityReference>("adx_forumthreadid").Equals(parentThread.ToEntityReference()) && p.GetAttributeValue<Guid>("adx_communityforumpostid") != currentPostId).OrderByDescending(p => p.GetAttributeValue<DateTime>("adx_date")).First();

				if (lastPost != null)
				{
					parentThread["adx_lastpostid"] = lastPost.ToEntityReference();
				}
			}

			if (currentPostId == currentFirstPostId)
			{
				var firstPost = GetForumPosts(context).Where(p => p.GetAttributeValue<EntityReference>("adx_forumthreadid") != null && p.GetAttributeValue<EntityReference>("adx_forumthreadid").Equals(parentThread.ToEntityReference()) && p.GetAttributeValue<Guid>("adx_communityforumpostid") != currentPostId).OrderBy(p => p.GetAttributeValue<DateTime>("adx_date")).First();

				if (firstPost != null)
				{
					parentThread["adx_firstpostid"] = firstPost.ToEntityReference();
				}
			}

			parentThread["adx_postcount"] = parentThread.GetAttributeValue<int>("adx_postcount") > 0 ? parentThread.GetAttributeValue<int>("adx_postcount") - 1 : 0;

			context.UpdateObject(parentThread);

			context.SaveChanges();
		}

		#endregion

		#region Forum

		public static IEnumerable<Entity> GetOrderedForumThreads(this OrganizationServiceContext context, Entity forum)
		{
			forum.AssertEntityName("adx_communityforum");

			var forumThreads = forum.GetRelatedEntities(context, "adx_communityforum_communityforumthread");

			var threads =
				from t in forumThreads
				orderby GetDate(context, t) descending
				select t;

			return threads;
		}

		private static DateTime? GetDate(OrganizationServiceContext context, Entity thread)
		{
			var lastPost = thread.GetRelatedEntity(context, "adx_communityforumpost_communityforumthread");

			if (lastPost != null)
			{
				return lastPost.GetAttributeValue<DateTime?>("adx_date");
			}

			var firstPost = thread.GetRelatedEntity(context, "adx_communityforumthrea_firstpost");

			if (firstPost != null)
			{
				return firstPost.GetAttributeValue<DateTime?>("adx_date");
			}

			return null;
		}

		#endregion

		#region Forum Threads

		public static bool GetCurrentUserHasAlert(this OrganizationServiceContext context, Entity forumThread)
		{
			forumThread.AssertEntityName("adx_communityforumthread");

			var contact = PortalContext.Current.User;

			if (contact != null)
			{
				var forumAlerts = forumThread.GetRelatedEntities(context, "adx_communityforumthread_communityforumaalert");

				var alerts =
					from e in forumAlerts
					where e.GetAttributeValue<EntityReference>("adx_threadid") == forumThread.ToEntityReference() && e.GetAttributeValue<EntityReference>("adx_subscriberid") == contact.ToEntityReference()
					select e;

				if (alerts.Count() > 0)
				{
					return true;
				}
			}
			return false;
		}

		public static IEnumerable<Entity> GetOrderedForumPosts(this OrganizationServiceContext context, Entity forumThread)
		{
			forumThread.AssertEntityName("adx_communityforumthread");

			var forumPosts = forumThread.GetRelatedEntities(context, "adx_communityforumthread_communityforumpost");

			return forumPosts.OrderBy(fp => fp.GetAttributeValue<DateTime?>("adx_date"));
		}

		#endregion
	}
}
