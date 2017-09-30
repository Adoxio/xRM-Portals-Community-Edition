/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// This will be the general Activity-Enabled Entity Data Adapter that will eventually replace the RatingDataAdapter.  All entities that have activities enabled
	/// will be able to have data adapter(s) built for them that will inherit from this data adapter, which will implment the IDataAdapter interfaces for all
	/// custom activity entities produced from this time moving forward.  Thus this will implement IRatingDataAdapter, IAlertSubscription Data Adapter, etc.
	/// </summary>
	public class ActivityEnabledEntityDataAdapter : IAlertSubscriptionDataAdapter
	{
		public ActivityEnabledEntityDataAdapter(EntityReference recordReference, IDataAdapterDependencies dependencies)
		{
			if (recordReference == null)
			{
				throw new ArgumentNullException("recordReference");
			}

			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			RecordReference = recordReference;
			Dependencies = dependencies;

			var serviceContext = Dependencies.GetServiceContext();
			
		}

		public ActivityEnabledEntityDataAdapter(ActivityEnabledEntity record, IDataAdapterDependencies dependencies)
			: this(record.EntityReference, dependencies) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference RecordReference { get; private set; }

		public void CreateAlert(EntityReference user)
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();

			var alert = CreateAlertEntity(user, serviceContext);
			
			serviceContext.AddObject(alert);
			serviceContext.SaveChanges();

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}:{1}", user.LogicalName, user.Id));
			
		}

		public void CreateAlert(EntityReference user, string regardingurl, string regardingid)
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();

			var alert = CreateAlertEntity(user, serviceContext);

			alert.Attributes["adx_regardingurl"] = regardingurl;
			alert.Attributes["adx_regardingid"] = regardingid;

			serviceContext.AddObject(alert);
			serviceContext.SaveChanges();

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}:{1}", user.LogicalName, user.Id));

		}

		private Entity CreateAlertEntity(EntityReference user, OrganizationServiceContext serviceContext)
		{
			if (user == null) throw new ArgumentNullException("user");

			if (user.LogicalName != "contact")
			{
                throw new ArgumentException(string.Format("Value must have logical name '{0}'", user.LogicalName), "user");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}:{1}", user.LogicalName, user.Id));


			var existingAlert = SelectAlert(serviceContext, user);

			if (existingAlert != null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}:{1}, Alert Exists", user.LogicalName, user.Id));

				return null;
			}

			var metadataRequest = new RetrieveEntityRequest
									  {
										  LogicalName = RecordReference.LogicalName,
										  EntityFilters = EntityFilters.Attributes
									  };

			var metadataResponse = (RetrieveEntityResponse)serviceContext.Execute(metadataRequest);

			var primaryIdFieldName = metadataResponse.EntityMetadata.PrimaryIdAttribute;

			var primaryNameFieldName = metadataResponse.EntityMetadata.PrimaryNameAttribute;

			var record = serviceContext.CreateQuery(RecordReference.LogicalName).FirstOrDefault(
				r => r.GetAttributeValue<Guid>(primaryIdFieldName) == RecordReference.Id);

			var contact =
				serviceContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == user.Id);

			var alert = new Entity("adx_alertsubscription");

			alert["regardingobjectid"] = RecordReference;

			//assign the contact to the customer s list
			var activityparty = new Entity("activityparty");

			activityparty["partyid"] = user;

			var particpant = new EntityCollection(new List<Entity> { activityparty });

			alert["customers"] = particpant;

			alert["subject"] = string.Format(ResourceManager.GetString("Subscription_Added_Alert_Message"), contact.GetAttributeValue<string>("fullname"),
			                                 record.GetAttributeValue<string>(primaryNameFieldName));
			return alert;
		}

		public void DeleteAlert(EntityReference user)
		{
			if (user == null) throw new ArgumentNullException("user");

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}:{1}", user.LogicalName, user.Id));

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var existingAlert = SelectAlert(serviceContext, user);

			if (existingAlert == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}:{1}, Alert Not Found", user.LogicalName, user.Id));

				return;
			}

			if (!serviceContext.IsAttached(existingAlert))
			{
				serviceContext.Attach(existingAlert);
			}

			serviceContext.DeleteObject(existingAlert);
			serviceContext.SaveChanges();
		}

		public bool HasAlert(EntityReference user)
		{
			if (user == null) throw new ArgumentNullException("user");

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}:{1}", user.LogicalName, user.Id));

			var serviceContext = Dependencies.GetServiceContext();
			var existingAlert = SelectAlert(serviceContext, user);

			var hasAlert = existingAlert != null;

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}:{1}, {2}", user.LogicalName, user.Id, hasAlert));

			return hasAlert;
		}

		protected Entity SelectAlert(OrganizationServiceContext serviceContext, EntityReference user)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (user == null) throw new ArgumentNullException("user");

			var metadataRequest = new RetrieveEntityRequest
												  {
													  LogicalName = RecordReference.LogicalName,
													  EntityFilters = EntityFilters.Attributes
												  };

			var metadataResponse = (RetrieveEntityResponse)serviceContext.Execute(metadataRequest);

			var primaryIdFieldName = metadataResponse.EntityMetadata.PrimaryIdAttribute;

			var fetchXmlString = string.Format(@"
					<fetch mapping=""logical"" distinct=""true"">
						<entity name=""adx_alertsubscription"">
							<attribute name=""activityid""/>
							<attribute name=""subject""/>
							<attribute name=""createdon""/>
							<link-entity name=""activityparty"" from=""activityid"" to=""activityid"" alias=""aa"">
								<filter type=""and"">
									<condition attribute=""partyid"" operator=""eq"" />
								</filter>
							</link-entity>
							<link-entity name=""{0}"" from=""{1}"" to=""regardingobjectid"" alias=""ab"">
								<filter type=""and"">
									<condition attribute=""{2}"" operator=""eq"" />
								</filter>
							</link-entity>
						</entity>
					</fetch>", RecordReference.LogicalName, primaryIdFieldName, primaryIdFieldName);

			var fetchXml = XDocument.Parse(fetchXmlString);

			var recordIdAttribute = fetchXml.XPathSelectElement(string.Format("//condition[@attribute='{0}']", primaryIdFieldName));

			if (recordIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the regarding record filter element.");
			}

			recordIdAttribute.SetAttributeValue("value", RecordReference.Id.ToString());

			var contactIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='partyid']");

			if (contactIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the contact filter element.");
			}

			contactIdAttribute.SetAttributeValue("value", user.Id.ToString());

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return response.EntityCollection.Entities.FirstOrDefault();

		}
	}
}
