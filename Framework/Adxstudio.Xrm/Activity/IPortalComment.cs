/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Notes;
using System.Collections;

namespace Adxstudio.Xrm.Activity
{
	public interface IPortalComment
	{
		string Subject { get; set; }
		string Description { get; set; }
		StateCode StateCode { get; set; }
		StatusCode StatusCode { get; set; }
		PortalCommentDirectionCode DirectionCode { get; set; }
		Guid ActivityId { get; set; }
		Entity From { get; set; }
		Entity To { get; set; }
		IEnumerable<IAnnotationFile> FileAttachments { get; set; }
		IAnnotationSettings AttachmentSettings { get; set; }
		EntityReference Regarding { get; set; }
		Entity Entity { get; set; }
	}

	public enum PortalCommentDirectionCode
	{
		Incoming = 1,
		Outgoing = 2
	}
	public enum StatusCode
	{
		Open = 1,
		Sent = 2,
		Received = 3,
		Canceled = 4,
		Scheduled = 5
	}
}
