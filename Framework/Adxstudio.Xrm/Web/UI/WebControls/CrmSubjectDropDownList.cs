/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Dropdown populated with Subjects from CRM.
	/// </summary>
	[ToolboxData("<{0}:CrmSubjectDropDownList runat=server></{0}:CrmSubjectDropDownList>")]
	public class CrmSubjectDropDownList : DropDownList
	{
		/// <summary>
		/// The name used to retrieve the configured Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
		/// </summary>
		public string ContextName
		{
			get { return ViewState["ContextName"] as string; }
			set { ViewState["ContextName"] = value; }
		}

		override protected void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Items.Count > 0)
			{
				return;
			}

			Items.Add(new ListItem(string.Empty, string.Empty));

			var context = CrmConfigurationManager.CreateContext(ContextName);

			var subjects = context.CreateQuery("subject").ToList();

			var parents = subjects.Where(s => s.GetAttributeValue<EntityReference>("parentsubject") == null).OrderBy(s => s.GetAttributeValue<string>("title"));

			foreach (var parent in parents)
			{
				if (parent == null)
				{
					continue;
				}

				Items.Add(new ListItem(parent.GetAttributeValue<string>("title"), parent.Id.ToString()));

				var parentId = parent.Id;

				var children = subjects.Where(s => s.GetAttributeValue<EntityReference>("parentsubject") != null && s.GetAttributeValue<EntityReference>("parentsubject").Id == parentId).OrderBy(s => s.GetAttributeValue<string>("title"));

				AddChildItems(subjects, children, 1);
			}
		}

		protected void AddChildItems(List<Entity> subjects, IEnumerable<Entity> children, int depth)
		{
			foreach (var child in children)
			{
				if (child == null)
				{
					continue;
				}

				var padding = HttpUtility.HtmlDecode(string.Concat(Enumerable.Repeat("&nbsp;-&nbsp;", depth)));

				Items.Add(new ListItem(string.Format("{0}{1}", padding, child.GetAttributeValue<string>("title")), child.Id.ToString()));

				var childId = child.Id;

				var grandchildren = subjects.Where(s => s.GetAttributeValue<EntityReference>("parentsubject") != null && s.GetAttributeValue<EntityReference>("parentsubject").Id == childId).OrderBy(s => s.GetAttributeValue<string>("title"));

				depth++;

				AddChildItems(subjects, grandchildren, depth);

				depth--;
			}
		}
	}
}
