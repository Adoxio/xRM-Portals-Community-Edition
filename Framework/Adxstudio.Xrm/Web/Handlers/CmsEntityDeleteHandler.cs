/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Providers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using PortalConfigurationDataAdapterDependencies = Adxstudio.Xrm.Forums.PortalConfigurationDataAdapterDependencies;
using Adxstudio.Xrm.Core.Flighting;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityDeleteHandler : CmsEntityHandler, System.Web.SessionState.IReadOnlySessionState
	{
		public CmsEntityDeleteHandler() { }

		public CmsEntityDeleteHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id) : base(portalName, portalScopeId, entityLogicalName, id) { }

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, ICrmEntitySecurityProvider security)
		{
			if (!(IsRequestMethod(context.Request, "POST") || IsRequestMethod(context.Request, "DELETE")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.MethodNotAllowed, "Request method {0} not allowed for this resource.".FormatWith(context.Request.HttpMethod));
			}

			CrmEntityInactiveInfo inactiveInfo;

			if (CrmEntityInactiveInfo.TryGetInfo(entity.LogicalName, out inactiveInfo))
			{
				serviceContext.SetState(inactiveInfo.InactiveState, inactiveInfo.InactiveStatus, entity);

				WriteNoContentResponse(context.Response);

				return;
			}

			if (entity.LogicalName == "adx_communityforumthread")
			{
				var forum = entity.GetRelatedEntity(serviceContext, new Relationship("adx_communityforum_communityforumthread"));

				if (forum != null)
				{
					var forumDataAdapter = new ForumDataAdapter(
						forum.ToEntityReference(),
						new PortalConfigurationDataAdapterDependencies(portalName: PortalName, requestContext: context.Request.RequestContext));

					forumDataAdapter.DeleteThread(entity.ToEntityReference());

					WriteNoContentResponse(context.Response);

                    if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
                    {
                        PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forum, HttpContext.Current, "delete_forumthread", 1, entity.ToEntityReference(), "delete");
                    }

                    return;
				}
			}

			if (entity.LogicalName == "adx_communityforumpost")
			{
				var forumThread = entity.GetRelatedEntity(serviceContext, new Relationship("adx_communityforumthread_communityforumpost"));

				if (forumThread != null)
				{
					var forumThreadDataAdapter = new ForumThreadDataAdapter(
						forumThread.ToEntityReference(),
						new PortalConfigurationDataAdapterDependencies(portalName: PortalName, requestContext: context.Request.RequestContext));

					forumThreadDataAdapter.DeletePost(entity.ToEntityReference());

					WriteNoContentResponse(context.Response);

                    if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
                    {
                        PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forum, HttpContext.Current, "delete_forumpost", 1, entity.ToEntityReference(), "delete");
                    }

                    return;
				}
			}
			
			serviceContext.DeleteObject(entity);
			serviceContext.SaveChanges();

			WriteNoContentResponse(context.Response);

            if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
            {
                PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Blog, HttpContext.Current, "delete_blogpost", 1, entity.ToEntityReference(), "delete");
            }
        }
    }
}
