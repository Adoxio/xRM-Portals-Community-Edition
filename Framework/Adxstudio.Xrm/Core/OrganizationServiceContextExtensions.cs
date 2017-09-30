/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Services;

namespace Adxstudio.Xrm.Core
{
	public static class OrganizationServiceContextExtensions
	{
		private static string EnsureValidFileName(string fileName)
		{
			return fileName.IndexOf("\\") >= 0 ? fileName.Substring(fileName.LastIndexOf("\\") + 1) : fileName;
		}

		public static string GetOptionSetValueLabel(this OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName, OptionSetValue optionSetValue)
		{
			return serviceContext.GetOptionSetValueLabel(entityLogicalName, attributeLogicalName, optionSetValue == null ? null : new int?(optionSetValue.Value));
		}

		public static string GetOptionSetValueLabel(this OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName, int? optionSetValue)
		{
			return GetOptionSetValueLabel(serviceContext, entityLogicalName, attributeLogicalName, optionSetValue, null);
		}

	    public static string GetOptionSetValueLabel(AttributeMetadata attributeMetadata, int? optionSetValue,
	        int? languageCode)
	    {
            var enumMetadata = attributeMetadata as EnumAttributeMetadata;

            if (enumMetadata == null)
            {
                return null;
            }

            var option = enumMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == optionSetValue.Value);

            if (option == null)
            {
                return string.Empty;
            }

            var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == (languageCode ?? 0));

            var label = localizedLabel == null ? option.Label.GetLocalizedLabelString() : localizedLabel.Label;

            return label;
        }

		public static string GetOptionSetValueLabel(this OrganizationServiceContext serviceContext, string entityLogicalName,
		                                            string attributeLogicalName, int? optionSetValue, int? languageCode)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (entityLogicalName == null) throw new ArgumentNullException("entityLogicalName");
			if (attributeLogicalName == null) throw new ArgumentNullException("attributeLogicalName");

			if (optionSetValue == null)
			{
				return null;
			}

			var response = (RetrieveAttributeResponse)serviceContext.Execute(new RetrieveAttributeRequest
			{
				EntityLogicalName = entityLogicalName,
				LogicalName = attributeLogicalName,
			});

			if (response == null)
			{
				return null;
			}

			var enumMetadata = response.AttributeMetadata as EnumAttributeMetadata;

			if (enumMetadata == null)
			{
				return null;
			}

			var option = enumMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == optionSetValue.Value);

			if (option == null)
			{
				return string.Empty;
			}

			var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == (languageCode ?? 0));
			
			var label = localizedLabel == null ? option.Label.GetLocalizedLabelString() : localizedLabel.Label;

			return label;
		}

		/// <summary>
		/// Execute RetrieveEntityRequest to get entity metadata.
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="entityLogicalName">Entity logical name</param>
		/// <param name="entityFilters">EntityFilters</param>
		/// <returns></returns>
		public static EntityMetadata GetEntityMetadata(this OrganizationServiceContext serviceContext, string entityLogicalName, EntityFilters entityFilters)
		{
			return GetEntityMetadata(serviceContext as IOrganizationService, entityLogicalName, entityFilters);
		}

		/// <summary>
		/// Execute RetrieveEntityRequest to get entity metadata.
		/// </summary>
		/// <param name="service">The service</param>
		/// <param name="entityLogicalName">Entity logical name</param>
		/// <param name="entityFilters">EntityFilters</param>
		/// <returns></returns>
		public static EntityMetadata GetEntityMetadata(this IOrganizationService service, string entityLogicalName, EntityFilters entityFilters)
		{
			var retrieveEntityRequest = new RetrieveEntityRequest
			{
				LogicalName = entityLogicalName,
				EntityFilters = entityFilters
			};

			var response = service.ExecuteRequest(retrieveEntityRequest) as RetrieveEntityResponse;

			return response.EntityMetadata;
		}

		public static Entity GetOrganizationEntity(this OrganizationServiceContext serviceContext, string[] columns = null)
		{
			// when we make a switch to 2015 sdk, we should just replace this with the new RetrieveOrganizationRequest/Response.
			if (columns == null)
			{
				columns = new[] { "name" };
			}
			return ((RetrieveResponse)serviceContext.Execute(new RetrieveRequest
			{
				ColumnSet = new ColumnSet(columns),
				Target = new EntityReference("organization", ((WhoAmIResponse)serviceContext.Execute(new WhoAmIRequest())).OrganizationId)
			})).Entity;
		}
	}
}
