/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Routing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Activity
{
	public class ActivityMimeAttachmentDataAdapter
	{
		private readonly IDataAdapterDependencies _dependencies;

		public ActivityMimeAttachmentDataAdapter(IDataAdapterDependencies dependencies)
		{
			_dependencies = dependencies;
		}

		public IEnumerable<IAttachment> GetAttachments(Guid regardingId)
		{
			return RetrieveAttachments(regardingId);
		}

		public void DownloadAttachment(HttpContextBase context, Entity entity, Entity webfile = null)
		{
			var attachment = GetAttachment(entity);
			DownloadFromCRM(context, attachment, webfile);
		}

		private IAttachment GetAttachment(Entity entity)
		{
			return new Attachment(() => GetAttachmentFile(entity, entity.GetAttributeValue<EntityReference>("attachmentid").Id));
		}

		private void DownloadFromCRM(HttpContextBase context, IAttachment attachment, Entity webfile)
		{
			if (attachment == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var crmFile = attachment as Attachment;

			if (crmFile == null || crmFile.AttachmentBody == null || crmFile.AttachmentBody.Length == 0)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NoContent;
				return;
			}

			var data = attachment.AttachmentBody;

			var defaultCacheability = context.User.Identity.IsAuthenticated ? HttpCacheability.Private : HttpCacheability.Public;

			SetResponseParameters(context.Response, defaultCacheability, attachment.Entity, webfile, data);

			Utility.Write(context.Response, data);
		}

		private IAttachment GetAttachmentFile(Entity activityMimeAttachment, Guid id)
		{
			ulong fileSize;
			if (!ulong.TryParse(activityMimeAttachment.GetAttributeValue<int>("filesize").ToString(), out fileSize))
			{
				fileSize = 0;
			}
			FileSize attachmentSize = new FileSize(fileSize);

			Entity attachment = null;
            if (activityMimeAttachment.Attributes.ContainsKey("attachmentid"))
			{
				attachment = new Entity("activitymimeattachment", activityMimeAttachment.GetAttributeValue<EntityReference>("attachmentid").Id);
			}

			return new Attachment
			{
				AttachmentContentType = activityMimeAttachment.GetAttributeValue<string>("mimetype"),
				AttachmentFileName = activityMimeAttachment.GetAttributeValue<string>("filename"),
				AttachmentIsImage =
					(new List<string> { "image/jpeg", "image/gif", "image/png" }).Contains(
						activityMimeAttachment.GetAttributeValue<string>("mimetype")),
				AttachmentSize = attachmentSize,
				AttachmentSizeDisplay = attachmentSize.ToString(),
				AttachmentUrl = attachment == null ? string.Empty : attachment.GetFileAttachmentUrl(_dependencies.GetWebsite()),
				AttachmentBody = GetAttachmentBody(activityMimeAttachment, activityMimeAttachment.Id),
				Entity = activityMimeAttachment
			};
		}

		private byte[] GetAttachmentBody(Entity attachment, Guid id)
		{
			if (!attachment.Attributes.ContainsKey("body"))
			{
				return null;
			}

			//Get the string representation of the attachment body
			var body = attachment.GetAttributeValue<string>("body");

			//Encode into a byte array and return
			return Convert.FromBase64String(body);
		}

		private static QueryExpression BuildActivityMimeAttachmentsQuery(Guid regardingId)
		{
			// Query the activitymimeattachment for all related attachments
			var query = new QueryExpression("activitymimeattachment");
			query.ColumnSet.AddColumns("filename", "filesize", "mimetype", "objectid", "attachmentid");

			query.Criteria.AddCondition("objectid", ConditionOperator.Equal, regardingId.ToString());

			return query;
		}

		private IEnumerable<IAttachment> RetrieveAttachments(Guid regardingId)
		{
			QueryExpression query = BuildActivityMimeAttachmentsQuery(regardingId);

			// Execute the query
			var serviceContext = _dependencies.GetServiceContext();
			var retrieveMultipleResponse = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest { Query = query });

			// Project the response into Attachment object
			var attachmentCollection = retrieveMultipleResponse.EntityCollection.Entities.Select(attachment => GetAttachmentFile(attachment, attachment.Id));

			return attachmentCollection;
		}

		private static void SetResponseParameters(HttpResponseBase response, HttpCacheability defaultCacheability,
			Entity attachment, Entity webfile, ICollection<byte> data)
		{
			response.StatusCode = (int)HttpStatusCode.OK;
			response.ContentType = attachment.GetAttributeValue<string>("mimetype");

			var contentDispositionText = "inline";

			if (webfile != null)
			{
				var contentDispositionOptionSetValue = webfile.GetAttributeValue<OptionSetValue>("adx_contentdisposition");

				if (contentDispositionOptionSetValue != null)
				{
					switch (contentDispositionOptionSetValue.Value)
					{
						case 756150000: // inline
							contentDispositionText = "inline";
							break;
						case 756150001: // attachment
							contentDispositionText = "attachment";
							break;
						default:
							contentDispositionText = "inline";
							break;
					}
				}
			}

			if (string.Equals(response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(response.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
			{
				contentDispositionText = "attachment";
			}

			var contentDisposition = new StringBuilder(contentDispositionText);

			AppendFilenameToContentDisposition(attachment, contentDisposition);

			response.AppendHeader("Content-Disposition", contentDisposition.ToString());
			response.AppendHeader("Content-Length", data.Count.ToString(CultureInfo.InvariantCulture));

			var section = PortalCrmConfigurationManager.GetPortalCrmSection();
			var policy = section.CachePolicy.Annotation;

			Utility.SetResponseCachePolicy(policy, response, defaultCacheability);
		}

		private static void AppendFilenameToContentDisposition(Entity attachment, StringBuilder contentDisposition)
		{
			var filename = attachment.GetAttributeValue<string>("filename");

			if (string.IsNullOrEmpty(filename))
			{
				return;
			}

			// Escape any quotes in the filename. (There should rarely if ever be any, but still.)
			var escaped = filename.Replace(@"""", @"\""");

			// Quote the filename parameter value.
			contentDisposition.AppendFormat(@";filename=""{0}""", escaped);
		}
	}
}
