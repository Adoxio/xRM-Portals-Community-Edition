/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public class HtmlForumPostSubmission : IForumPostSubmission
	{
		public HtmlForumPostSubmission(string name, string htmlContent, DateTime postedOn, IForumAuthor author)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value can't be null or empty.", "name");
			if (postedOn.Kind != DateTimeKind.Utc) throw new ArgumentException("Value must be UTC.", "postedOn");
			if (author == null) throw new ArgumentNullException("author");

			Name = name;
			Content = SafeHtml.SafeHtmSanitizer.GetSafeHtml(htmlContent ?? string.Empty);
			PostedOn = postedOn;
			Author = author;
			Attachments = new List<IForumPostAttachment>();
		}

		public IEnumerable<IForumPostAttachmentInfo> AttachmentInfo
		{
			get { return new IForumPostAttachmentInfo[] { }; }
		}

		public ICollection<IForumPostAttachment> Attachments { get; private set; }

		public DateTime PostedOn { get; private set; }

		public EntityReference ThreadEntity { get; private set; }

		EntityReference IPortalViewEntity.EntityReference
		{
			get { return null; }
		}

		public string Url
		{
			get { return null; }
		}

		public string Content { get; private set; }

		public bool CanEdit
		{
			get { return false; }
		}

		public ApplicationPath DeletePath
		{
			get { return null; }
		}

		public ApplicationPath EditPath
		{
			get { return null; }
		}

		public Entity Entity
		{
			get { return null; }
		}

		public int HelpfulVoteCount
		{
			get { return 0; }
		}

		public bool IsAnswer { get; set; }

		public bool CanMarkAsAnswer { get; set; }

		public string Name { get; private set; }

		public IForumThread Thread { get; set; }

		public string Description
		{
			get { return string.Empty; }
		}

		public bool Editable
		{
			get { return false; }
		}

		public IForumAuthor Author { get; private set; }

		EntityReference IForumPostInfo.EntityReference
		{
			get { return null; }
		}

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			throw new NotSupportedException();
		}
	}

	public interface IForumPostAttachment
	{
		byte[] Content { get; }

		string ContentType { get; }

		string Name { get; }
	}

	public class ForumPostAttachment : IForumPostAttachment
	{
		public ForumPostAttachment(string name, string contentType, byte[] content)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (content == null) throw new ArgumentNullException("content");

			Name = Path.GetFileName(name);
			ContentType = contentType;
			Content = content;
		}

		public byte[] Content { get; private set; }

		public string ContentType { get; private set; }

		public string Name { get; private set; }
	}

	public class HtmlForumPostUpdate : IForumPostSubmission
	{
		private readonly EntityReference _entityReference;

		public HtmlForumPostUpdate(EntityReference entityReference, string name = null, string htmlContent = null)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			_entityReference = entityReference;

			Name = name;
			Content = htmlContent == null ? null : SafeHtml.SafeHtmSanitizer.GetSafeHtml(htmlContent);
			Attachments = new List<IForumPostAttachment>();
		}

		public IEnumerable<IForumPostAttachmentInfo> AttachmentInfo
		{
			get { return new IForumPostAttachmentInfo[] { }; }
		}

		public ICollection<IForumPostAttachment> Attachments { get; private set; }

		public DateTime PostedOn
		{
			get { return default(DateTime); }
		}

		public EntityReference ThreadEntity
		{
			get { return null; }
		}

		EntityReference IPortalViewEntity.EntityReference
		{
			get { return _entityReference; }
		}

		public string Url
		{
			get { return null; }
		}

		public string Content { get; private set; }

		public bool CanEdit
		{
			get { return false; }
		}

		public ApplicationPath DeletePath
		{
			get { return null; }
		}

		public ApplicationPath EditPath
		{
			get { return null; }
		}

		public Entity Entity
		{
			get { return null; }
		}

		public int HelpfulVoteCount
		{
			get { return 0; }
		}

		public bool IsAnswer
		{
			get { return false; }
		}

		public bool CanMarkAsAnswer
		{
			get { return false; }
		}

		public string Name { get; private set; }

		public IForumThread Thread
		{
			get { return null; }
		}

		public string Description
		{
			get { return string.Empty; }
		}

		public bool Editable
		{
			get { return false; }
		}

		public IForumAuthor Author
		{
			get { return null; }
		}

		EntityReference IForumPostInfo.EntityReference
		{
			get { return _entityReference; }
		}

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			throw new NotSupportedException();
		}
	}
}
