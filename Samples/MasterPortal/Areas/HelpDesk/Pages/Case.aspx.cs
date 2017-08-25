/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.HelpDesk.Pages
{
	public partial class Case : PortalPage
	{
		protected ICase CurrentCase { get; private set; }

		protected ICaseAccess CurrentCaseAccess { get; private set; }

		protected ICaseDataAdapter CurrentCaseDataAdapter { get; private set; }

		protected void Page_Init(object sender, EventArgs args)
		{
			Guid id;

			if (Guid.TryParse(Request.QueryString["caseid"], out id))
			{
				CurrentCaseDataAdapter = new CaseDataAdapter(new EntityReference("incident", id), new PortalContextDataAdapterDependencies(Portal, PortalName, Request.RequestContext));
				CurrentCase = CurrentCaseDataAdapter.Select();
				CurrentCaseAccess = CurrentCaseDataAdapter.SelectAccess();
			}
		}

		protected void Page_Load(object sender, EventArgs args)
		{
			if (CurrentCase == null)
			{
				CaseBreadcrumbs.Visible = false;
				CaseHeader.Visible = false;
				CaseData.Visible = false;
				CaseNotFound.Visible = true;

				return;
			}

			if (!(CurrentCaseAccess.Read || CurrentCaseAccess.Public))
			{
				CaseData.Visible = false;
				NoCaseAccess.Visible = true;

				return;
			}

			if (CurrentCaseAccess.Read && CurrentCase.ResponsibleContact != null)
			{
				UserAvatar.DataBind();
				UserName.DataBind();
			}
			else
			{
				UserAvatar.Visible = false;
				UserName.Visible = false;
			}

			// Only people with private read access see this info.
			TicketNumber.Visible
				= CaseInfo.Visible
				= Notes.Visible
				= CurrentCaseAccess.Read;

			CaseControls.Visible = CurrentCaseAccess.Write;
			ResolveCase.Visible = CancelCase.Visible = AddNote.Visible = AddNoteInline.Visible = CurrentCaseAccess.Write && CurrentCase.IsActive;
			ReopenCase.Visible = CurrentCaseAccess.Write && !CurrentCase.IsActive;

			var fetchXml = string.Format(_caseFetchXmlFormat, CurrentCase.EntityReference.Id);
			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", FetchXml = fetchXml, CrmDataContextName = PublicFormView.ContextName };

			if (CurrentCaseAccess.Public)
			{
				PublicForm.Visible = true;
				PublicForm.Controls.Add(formViewDataSource);
				PublicFormView.DataSourceID = "WebFormDataSource";
			}
			else
			{
				if (CurrentCase.IsActive)
				{
					PrivateOpenCaseForm.Visible = true;
					PrivateClosedCaseForm.Visible = false;
					PublicForm.Visible = false;
					PrivateOpenCaseForm.Controls.Add(formViewDataSource);
					PrivateOpenCaseFormView.DataSourceID = "WebFormDataSource";
				}
				else
				{
					PrivateClosedCaseForm.Visible = true;
					PrivateOpenCaseForm.Visible = false;
					PublicForm.Visible = false;
					PrivateClosedCaseForm.Controls.Add(formViewDataSource);
					PrivateClosedCaseFormView.DataSourceID = "WebFormDataSource";
				}
			}
		}

		protected void ResolveCase_Click(object sender, EventArgs args)
		{
			if (!CurrentCaseAccess.Write)
			{
                throw new InvalidOperationException("You don't have permission to perform this operation.");
			}

			int customerSatisfactionCode;

			if (!int.TryParse(Satisfaction.SelectedValue, out customerSatisfactionCode))
			{
				throw new InvalidOperationException("Unable to retrieve the customer satisfaction code.");
			}

			CurrentCaseDataAdapter.Resolve(customerSatisfactionCode, Resolution.Text, ResolutionDescription.Text);

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void CancelCase_Click(object sender, EventArgs args)
		{
			if (!CurrentCaseAccess.Write)
			{
                throw new InvalidOperationException("You don't have permission to perform this operation.");
			}

			CurrentCaseDataAdapter.Cancel();

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void ReopenCase_Click(object sender, EventArgs args)
		{
			if (!CurrentCaseAccess.Write)
			{
                throw new InvalidOperationException("You don't have permission to perform this operation.");
			}

			CurrentCaseDataAdapter.Reopen();

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void AddNote_Click(object sender, EventArgs args)
		{
			if (!CurrentCaseAccess.Write)
			{
                throw new InvalidOperationException("You don't have permission to perform this operation.");
			}

			if (string.IsNullOrWhiteSpace(NewNoteText.Text) && !NewNoteAttachment.HasFile)
			{
				Response.Redirect(Request.Url.PathAndQuery);
			}

			if (NewNoteAttachment.HasFile)
			{
				CurrentCaseDataAdapter.AddNote(NewNoteText.Text, NewNoteAttachment.PostedFile.FileName, NewNoteAttachment.PostedFile.ContentType, NewNoteAttachment.FileBytes);
			}
			else
			{
				CurrentCaseDataAdapter.AddNote(NewNoteText.Text);
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void GetCurrentCaseDataAdapter(object sender, ObjectDataSourceEventArgs args)
		{
			args.ObjectInstance = CurrentCaseDataAdapter;
		}

		protected void OnItemUpdated(object sender, EventArgs args)
		{
			UpdateSuccessMessage.Visible = true;
		}

		protected IHtmlString FormatTextAsHtml(string text)
		{
			return text == null ? null : new SimpleHtmlFormatter().Format(text);
		}

		private const string _caseFetchXmlFormat = @"
			<fetch mapping=""logical"">
				<entity name=""incident"">
					<all-attributes />
					<filter type=""and"">
						<condition attribute=""incidentid"" operator=""eq"" value=""{0}""/>
					</filter>
				</entity>
			</fetch>";
	}
}
