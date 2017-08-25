/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogPostComment : IBlogPostComment
	{
		private readonly Lazy<bool> _editable;
		private readonly Lazy<ApplicationPath> _getDeletePath;
		private readonly Lazy<ApplicationPath> _getEditPath;

		public BlogPostComment(Entity entity, string authorName, string authorUrl, string authorEmail,
			Lazy<ApplicationPath> getEditPath = null, Lazy<ApplicationPath> getDeletePath = null, Lazy<bool> editable = null, IRatingInfo ratingInfo = null,
			bool ratingEnabled = true)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "feedback")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", entity.LogicalName), "entity");
			}

			Entity = entity;

			Author = new Author(entity.GetAttributeValue<EntityReference>("createdbycontact"), authorName, authorEmail, authorUrl);
			
			AnchorName = GetAnchorName(entity.Id);

			IsApproved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

			RatingInfo = ratingInfo;

			_getEditPath = getEditPath;
			_getDeletePath = getDeletePath;
			_editable = editable;
			RatingEnabled = ratingEnabled;
		}

		public string AnchorName { get; private set; }

		public IAuthor Author { get; private set; }

		public ApplicationPath DeletePath
		{
			get { return _getDeletePath == null ? null : _getDeletePath.Value; }
		}

		public ApplicationPath EditPath
		{
			get { return _getEditPath == null ? null : _getEditPath.Value; }
		}
		
		public bool Editable
		{
			get { return _editable != null && _editable.Value; }
		}

		public Entity Entity { get; private set; }

		public bool IsApproved { get; private set; }

		public string Content
		{
			get { return Entity.GetAttributeValue<string>("comments"); }
		}

		public DateTime Date
		{
			get { return Entity.GetAttributeValue<DateTime>("createdon"); }
		}

		internal static string GetAnchorName(Guid commentId)
		{
			return "comment-{0}".FormatWith(commentId);
		}

		public IRatingInfo RatingInfo { get; private set; }

		public bool RatingEnabled { get; private set; }
	}
}
