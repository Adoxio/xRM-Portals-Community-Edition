/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Portal.Web.UI.WebControls;

namespace Site.Controls
{
	public partial class CommentCreator : PortalUserControl
	{

		protected void Page_Load(object sender, EventArgs e)
		{
			NewCommentAuthorInfoPanel.Visible = CommentsRequireAuthorInfo();
		}

		protected void NewComment_OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e)
		{
			var commentAdapterFactory = new CommentDataAdapterFactory(Entity.ToEntityReference());

			var dataAdapter = commentAdapterFactory.GetAdapter(Portal, Request.RequestContext);

			if (dataAdapter == null)
			{
				Visible = false;

				return;
			}

			var content = e.Values[dataAdapter.GetCommentContentAttributeName()];

			var sanitizedContent = SafeHtml.SafeHtmSanitizer.GetSafeHtml(content == null ? string.Empty : content.ToString());

			var attributes = dataAdapter.GetCommentAttributes(sanitizedContent, CommentAuthorName.Text, CommentAuthorEmail.Text, CommentAuthorUrl.Text, Context);

			foreach (var attribute in attributes)
			{
				e.Values[attribute.Key] = attribute.Value;
			}
		}

		protected void NewComment_OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void NewCommentFormView_OnPreRender(object sender, EventArgs e)
		{
			DisableClientScriptOnChildValidatorControls(NewCommentFormView);
		}

		protected bool CommentsRequireAuthorInfo()
		{
			return Portal.User == null || Portal.User.LogicalName != "contact";
		}

		private static void DisableClientScriptOnChildValidatorControls(Control control)
		{
			foreach (Control childControl in control.Controls)
			{
				var validator = childControl as BaseValidator;

				if (validator != null)
				{
					validator.EnableClientScript = false;

					continue;
				}

				DisableClientScriptOnChildValidatorControls(childControl);
			}
		}
	}
}
