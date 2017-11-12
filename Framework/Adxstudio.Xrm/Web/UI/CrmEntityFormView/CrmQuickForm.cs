/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	public class CrmQuickForm
	{
		public string DataFieldName { get; set; }

		public QuickFormId[] QuickFormIds { get; set; }

		public CrmQuickForm(string dataFieldName, QuickFormId[] quickFormIds)
		{
			DataFieldName = dataFieldName;

			QuickFormIds = quickFormIds;
		}

		public class QuickFormId
		{
			public Guid FormId { get; set; }

			public string EntityName { get; set; }

			public QuickFormId(string entityName, Guid formId)
			{
				EntityName = entityName;
				FormId = formId;
			}
		}
	}
}
