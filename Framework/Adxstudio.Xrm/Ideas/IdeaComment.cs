/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Represents a Comment for an Idea in an Adxstudio Portals Idea Forum.
	/// </summary>
	public class IdeaComment : IComment
	{
		private readonly Lazy<bool> _editable;
		private readonly Lazy<ApplicationPath> _getDeletePath;
		private readonly Lazy<ApplicationPath> _getEditPath;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entity">An feedback entity.</param>
		/// <param name="authorName">The name of the author for this comment.</param>
		/// <param name="authorEmail">The email of the author for this comment.</param>
		public IdeaComment(Entity entity, string authorName, string authorEmail, Lazy<ApplicationPath> getEditPath = null,
			Lazy<ApplicationPath> getDeletePath = null, Lazy<bool> editable = null, IRatingInfo ratingInfo = null,
			bool ratingEnabled = true)
		{
			entity.ThrowOnNull("entity");
			entity.AssertEntityName("feedback");

			Entity = entity;

			var authorId = entity.GetAttributeValue<EntityReference>(FeedbackMetadataAttributes.UserIdAttributeName) == null ? Guid.Empty : entity.GetAttributeValue<EntityReference>(FeedbackMetadataAttributes.UserIdAttributeName).Id;
			authorName = entity.GetAttributeValue<string>("adx_createdbycontact") ?? authorName;
			authorEmail = entity.GetAttributeValue<string>("adx_contactemail") ?? authorEmail;
			Author = authorId != Guid.Empty
				? new Author(new EntityReference("contact", authorId), authorName, authorEmail)
				: new Author(authorName, authorEmail);

			Content = entity.GetAttributeValue<string>("comments");
			PostedOn = entity.GetAttributeValue<DateTime?>("adx_date") ?? entity.GetAttributeValue<DateTime>("createdon");
			Name = entity.GetAttributeValue<string>("title");

			RatingInfo = ratingInfo;

			_getEditPath = getEditPath;
			_getDeletePath = getDeletePath;
			_editable = editable;
			RatingEnabled = ratingEnabled;
		}

		/// <summary>
		/// The author for this comment.
		/// </summary>
		public IAuthor Author { get; private set; }

		/// <summary>
		/// The comment copy.
		/// </summary>
		public string Content { get; private set; }

		public DateTime Date
		{
			get { return Entity.GetAttributeValue<DateTime>("createdon"); }
		}

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

		/// <summary>
		/// An feedback entity.
		/// </summary>
		public Entity Entity { get; private set; }

		/// <summary>
		/// Whether or not this comment should be visible in the portal.
		/// </summary>
		public bool IsApproved { get; private set; }

		/// <summary>
		/// A title for the comment.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// When the comment was posted.
		/// </summary>
		public DateTime PostedOn { get; private set; }

		public IRatingInfo RatingInfo { get; private set; }

		public bool RatingEnabled { get; private set; }
	}
}
