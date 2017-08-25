/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;

namespace Site.Controls
{
	public partial class Comments : PortalUserControl
	{
		public bool EnableRatings
		{
			get
			{
				var enabled = ViewState["EnableRatings"];

				return enabled != null && Convert.ToBoolean(enabled);
			}
			set
			{
				ViewState["EnableRatings"] = value;
			}
		}

		protected string VisitorID
		{
			get { return Context.Profile.UserName; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				CommentsView.DataBind();
			}

			var commentAdapterFactory = new CommentDataAdapterFactory(Entity.ToEntityReference());

			var dataAdapter = commentAdapterFactory.GetAdapter(Portal, Request.RequestContext);

			if (dataAdapter == null)
			{
				Visible = false;

				return;
			}

			var commentPolicyReader = dataAdapter.GetCommentPolicyReader();

			CommentsView.Visible = (!commentPolicyReader.IsCommentPolicyNone);

			NewCommentPanel.Visible = CommentsOpenToCurrentUser(commentPolicyReader);
		}

		protected void CreateCommentDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			var commentAdapterFactory = new CommentDataAdapterFactory(Entity.ToEntityReference());

			e.ObjectInstance = commentAdapterFactory.GetAdapter(Portal, Request.RequestContext);
		}

		protected bool CommentsOpenToCurrentUser(ICommentPolicyReader policyReader)
		{
			if (policyReader.IsCommentPolicyOpen || policyReader.IsCommentPolicyModerated)
			{
				return true;
			}

			//to maintain compatibilty
			return Context.User != null
				&& Context.User.Identity.IsAuthenticated
				&& (policyReader.IsCommentPolicyOpenToAuthenticatedUsers);
		}

		protected bool CommentsRequireAuthorInfo()
		{
			return Portal.User == null || Portal.User.LogicalName != "contact";
		}

	}
}
