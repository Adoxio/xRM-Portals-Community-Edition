/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents full, extended info about sales literature. 
	/// </summary>
	public class SalesLiterature : ISalesLiterature
	{
		/// <summary>
		/// SalesLiterature initialization
		/// </summary>
		/// <param name="literature">Sales Literature entity record</param>
		/// <param name="literatureMetadata">Sales Literature entity metadata</param>
		public SalesLiterature(Entity literature, EntityMetadata literatureMetadata)
		{
			if (literature == null) throw new ArgumentNullException("literature");
			if (literatureMetadata == null) throw new ArgumentNullException("literatureMetadata");
			if (literature.LogicalName != "salesliterature") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), literature.LogicalName), "literature");

			Entity = literature;
			EntityReference = literature.ToEntityReference();
			LiteratureTypeCodeLabel = literature.GetEnumLabel(literatureMetadata, "literaturetypecode", null);
			EmployeeContact = literature.GetAttributeValue<EntityReference>("employeecontactid");
			Subject = literature.GetAttributeValue<EntityReference>("subjectid");
		}

		public string Description { get { return Entity.GetAttributeValue<string>("description"); } }
		public DateTime ExpirationDate { get { return Entity.GetAttributeValue<DateTime>("expirationdate"); } }
		public EntityReference EmployeeContact { get; private set; }
		public Entity Entity { get; private set; }
		public EntityReference EntityReference { get; private set; }
		public bool HasAttachments { get { return Entity.GetAttributeValue<bool>("hasattachments"); } }
		public bool IsCustomerViewable { get { return Entity.GetAttributeValue<bool>("iscustomerviewable"); } }
		public string Keywords { get { return Entity.GetAttributeValue<string>("keywords"); } }
		public SalesLiteratureTypeCode? LiteratureTypeCode
		{
			get
			{
				var option = Entity.GetAttributeValue<OptionSetValue>("literaturetypecode");
				if (option == null)
				{
					return null;
				}
				var type =  option.Value;
				return Enum.IsDefined(typeof(SalesLiteratureTypeCode), type)
					       ? (SalesLiteratureTypeCode?)((SalesLiteratureTypeCode)type)
					       : null;
			}
		}
		public string LiteratureTypeCodeLabel { get; private set; }
		public string Name { get { return Entity.GetAttributeValue<string>("name"); } }
		public EntityReference Subject { get; private set; }
	}
}
