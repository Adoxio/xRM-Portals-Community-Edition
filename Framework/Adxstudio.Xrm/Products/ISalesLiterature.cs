/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	public interface ISalesLiterature
	{
		string Description { get; }

		DateTime ExpirationDate { get; }

		EntityReference EmployeeContact { get; }

		Entity Entity { get; }

		EntityReference EntityReference { get; }

		bool HasAttachments { get; }

		bool IsCustomerViewable { get; }

		string Keywords { get; }

		SalesLiteratureTypeCode? LiteratureTypeCode { get; }

		string LiteratureTypeCodeLabel { get; }

		string Name { get; }

		EntityReference Subject { get; }
	}
}
