/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Notes;
using System.Collections;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Activity
{
	public sealed class PortalComment : Activity, IPortalComment
	{
		public PortalComment()
		{
		}

		public PortalComment(Entity entity, EntityReference regarding) : base(entity, regarding)
		{
		}

		public string Subject { get; set; }
		public string Description { get; set; }
		public StateCode StateCode { get; set; }
		public StatusCode StatusCode { get; set; }
		public PortalCommentDirectionCode DirectionCode { get; set; }
		public Entity From { get; set; }
		public Entity To { get; set; }
		public new IEnumerable<IAnnotationFile> FileAttachments { get; set; }
		public IAnnotationSettings AttachmentSettings { get; set; }
		public Guid ActivityId { get; set; }
	}
}
