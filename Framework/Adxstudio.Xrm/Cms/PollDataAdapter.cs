/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Reflection;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms
{
	public class PollDataAdapter : IPollDataAdapter
	{
		public const string PollRoute = "Poll";
		public const string PlacementRoute = "PollPlacement";
		public const string RandomPollRoute = "RandomPoll";
		public const string SubmitPollRoute = "SubmitPoll";

		public PollDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Dependencies = dependencies;
		}

		public IDataAdapterDependencies Dependencies { get; private set; }

		public IPoll SelectPoll(Guid pollId)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", pollId));

			var poll = SelectPoll(e => e.GetAttributeValue<Guid>("adx_pollid") == pollId);

			if (poll == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", pollId));

			return poll;
		}

		public IPoll SelectPoll(string pollName)
		{
			if (string.IsNullOrEmpty(pollName))
			{
				return null;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var poll = SelectPoll(e => e.GetAttributeValue<string>("adx_name") == pollName);

			if (poll == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return poll;
		}

		protected IPoll SelectPoll(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();
			var website = Dependencies.GetWebsite();

			// Bulk-load all poll entities into cache.
			var allEntities = serviceContext.CreateQuery("adx_poll")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid").Id == website.Id)
				.ToArray();

			var now = DateTime.UtcNow;
			var entity = allEntities.FirstOrDefault(e => match(e) && IsActive(e)
				&& ((DateTime.Compare(e.GetAttributeValue<DateTime?>("adx_releasedate") ?? now, now.AddDays(1))) <= 0
					&& (DateTime.Compare(e.GetAttributeValue<DateTime?>("adx_expirationdate") ?? now, now)) >= 0));

			if (entity == null)
			{
				return null;
			}

			/*
			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				return null;
			}
			*/

			return CreatePoll(entity, serviceContext);
		}

		public IPollPlacement SelectPollPlacement(Guid pollPlacementId)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}",
				pollPlacementId));

			var adPlacement = SelectPollPlacement(e => e.GetAttributeValue<Guid>("adx_pollplacementid") == pollPlacementId);

			if (adPlacement == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}",
				pollPlacementId));

			return adPlacement;
		}

		public IPollPlacement SelectPollPlacement(string pollPlacementName)
		{
			if (string.IsNullOrEmpty(pollPlacementName))
			{
				return null;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var pollPlacement = SelectPollPlacement(e => e.GetAttributeValue<string>("adx_name") == pollPlacementName);

			if (pollPlacement == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return pollPlacement;
		}

		protected IPollPlacement SelectPollPlacement(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();
			var website = Dependencies.GetWebsite();

			// Bulk-load all poll placement entities into cache.
			var allEntities = serviceContext.CreateQuery("adx_pollplacement")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid").Id == website.Id)
				.ToArray();

			var entity = allEntities.FirstOrDefault(e => match(e) && IsActive(e));

			if (entity == null)
			{
				return null;
			}

			/*
			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				return null;
			}
			*/

			return CreatePollPlacement(entity, serviceContext);
		}

		public IPoll SelectRandomPoll(Guid pollPlacementId)
		{
			var placement = SelectPollPlacement(pollPlacementId);
			return placement == null ? null : SelectRandomPoll(placement);
		}

		public IPoll SelectRandomPoll(string pollPlacementName)
		{
			var placement = SelectPollPlacement(pollPlacementName);
			return placement == null ? null : SelectRandomPoll(placement);
		}

		public void SubmitPoll(IPoll poll, IPollOption pollOption)
		{
			if (poll == null)
			{
				throw new InvalidOperationException("Unable to retrieve active poll.");
			}


			if (HasUserVoted(poll) || pollOption == null)
			{
				return;
			}

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var pollClone = serviceContext.CreateQuery("adx_poll")
				.FirstOrDefault(p => p.GetAttributeValue<Guid>("adx_pollid") == poll.Entity.Id);

			if (pollClone == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current poll.");
			}

			var optionClone = serviceContext.CreateQuery("adx_polloption")
				.FirstOrDefault(o => o.GetAttributeValue<Guid>("adx_polloptionid") == pollOption.Entity.Id);

			if (optionClone == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current poll option.");
			}

			var user = Dependencies.GetPortalUser();

			var visitorId = Dependencies.GetRequestContext().HttpContext.Profile.UserName;

			if (user != null && user.LogicalName == "contact")
			{
				var contact =
					serviceContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == user.Id);

				if (contact == null)
				{
					throw new InvalidOperationException("Unable to retrieve the current user contact.");
				}

				var submission = new Entity("adx_pollsubmission");

				submission.SetAttributeValue("adx_name", ResourceManager.GetString("Poll_Submission_For_Message") + contact.GetAttributeValue<string>("fullname"));

				serviceContext.AddObject(submission);

				serviceContext.AddLink(submission, "adx_contact_pollsubmission".ToRelationship(), contact);
				serviceContext.AddLink(submission, "adx_polloption_pollsubmission".ToRelationship(), optionClone);
				serviceContext.AddLink(submission, "adx_poll_pollsubmission".ToRelationship(), pollClone);

				IncrementVoteCount(serviceContext, pollOption);

				serviceContext.SaveChanges();
			}
			else if (!string.IsNullOrEmpty(visitorId))
			{
				var submission = new Entity("adx_pollsubmission");

				submission.SetAttributeValue("adx_visitorid", visitorId);
				submission.SetAttributeValue("adx_name", "Poll submission for " + visitorId);

				serviceContext.AddObject(submission);

				serviceContext.AddLink(submission, "adx_polloption_pollsubmission".ToRelationship(), optionClone);
				serviceContext.AddLink(submission, "adx_poll_pollsubmission".ToRelationship(), pollClone);

				IncrementVoteCount(serviceContext, pollOption);

				serviceContext.SaveChanges();
			}
		}

		private void IncrementVoteCount(OrganizationServiceContext serviceContext, IPollOption pollOption)
		{
			var option = serviceContext.CreateQuery("adx_polloption")
				.FirstOrDefault(o => o.GetAttributeValue<Guid>("adx_polloptionid") == pollOption.Entity.Id);

			if (option == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current poll option.");
			}

			int? voteCount = option.GetAttributeValue<int?>("adx_votes") ?? 0;

			option.SetAttributeValue("adx_votes", (voteCount + 1));

			serviceContext.UpdateObject(option);
		}

		public bool HasUserVoted(IPoll poll)
		{
			return poll.UserSelectedOption != null;
		}

		protected IPoll SelectRandomPoll(IPollPlacement placement)
		{
			if (placement == null)
			{
				return null;
			}

			var array = placement.Polls.ToArray();

			if (array.Length == 0) return null;

			var random = new Random(DateTime.Now.Millisecond);
			return array[random.Next(0, array.Length)];
		}

		private IPoll CreatePoll(Entity entity, OrganizationServiceContext serviceContext)
		{
			var user = Dependencies.GetPortalUser();
			var anon = Dependencies.GetRequestContext().HttpContext.Profile.UserName;

			var poll = new Poll(entity);

			poll.Options = entity.GetRelatedEntities(serviceContext, new Relationship("adx_poll_polloption"))
				.Select(e => CreatePollOption(poll, e));

			var submitted = entity.GetRelatedEntities(serviceContext, new Relationship("adx_poll_pollsubmission"))
				.FirstOrDefault(e => user != null
					&& user.LogicalName == "contact"
					&& e.GetAttributeValue<EntityReference>("adx_contactid") != null
					? e.GetAttributeValue<EntityReference>("adx_contactid").Id == user.Id
					: e.GetAttributeValue<string>("adx_visitorid") == anon);

			poll.UserSelectedOption = submitted != null
				? new PollOption(submitted.GetRelatedEntity(serviceContext, "adx_polloption_pollsubmission"), poll)
				: null;

			return poll;
		}

		private IPollOption CreatePollOption(IPoll poll, Entity entity)
		{
			return new PollOption(entity, poll);
		}

		private IPollPlacement CreatePollPlacement(Entity entity, OrganizationServiceContext serviceContext)
		{
			var polls = entity.GetRelatedEntities(serviceContext, new Relationship("adx_pollplacement_poll"))
				.Where(e => ((DateTime.Compare(e.GetAttributeValue<DateTime?>("adx_releasedate") ?? DateTime.UtcNow,
					DateTime.UtcNow.AddDays(1))) <= 0
					&& (DateTime.Compare(e.GetAttributeValue<DateTime?>("adx_expirationdate") ?? DateTime.UtcNow,
						DateTime.UtcNow)) >= 0))
				.Where(IsActive)
				.Select(e => CreatePoll(e, serviceContext));

			var pollPlacement = new PollPlacement(entity, polls);

			return pollPlacement;
		}

		private static bool IsActive(Entity entity)
		{
			if (entity == null)
			{
				return false;
			}

			var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

			return statecode != null && statecode.Value == 0;
		}
	}
}
