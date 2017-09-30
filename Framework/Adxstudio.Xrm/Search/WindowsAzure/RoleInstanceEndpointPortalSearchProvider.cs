/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Search.Index;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Resources;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Search.WindowsAzure
{
	/// <summary>
	/// <see cref="SearchProvider"/> which consumes the search index service published by <see cref="CloudDriveServiceSearchProvider"/>,
	/// and provides <see cref="PortalContext"/>-based filter, security validation, etc. for the results returned by that service.
	/// </summary>
	/// <remarks>
	/// This provider requires the following configuration settings to be a part of the role definition/configuration:
	/// 
	/// - Adxstudio.Xrm.Search.WindowsAzure.ServiceRole: The name of the Windows Azure role which uses <see cref="CloudDriveServiceSearchProvider"/>
	///   to publish <see cref="ISearchService"/> on an internal endpoint.
	/// 
	/// - Adxstudio.Xrm.Search.WindowsAzure.ServiceEndpoint: The name of the internal endpoint on which the search
	///   index service will be available, and is part of the definition/configuration of the role specified by
	///   Adxstudio.Xrm.Search.WindowsAzure.ServiceRole. This endpoint must have protocol="tcp".
	/// </remarks>
	public class RoleInstanceEndpointPortalSearchProvider : SearchProvider
	{
		protected string BindingConfiguration { get; private set; }

		protected string PortalName { get; private set; }

		protected ChannelFactory<ISearchService> ServiceChannelFactory { get; private set; }

		protected EndpointAddress ServiceEndpointAddress { get; private set; }

		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			if (string.IsNullOrEmpty(name))
			{
				name = GetType().Name;
			}

			base.Initialize(name, config);

			PortalName = config["portalName"];
			BindingConfiguration = config["bindingConfiguration"];

			var recognizedAttributes = new List<string>
			{
				"name",
				"description",
				"portalName",
				"bindingConfiguration"
			};

			// Remove all of the known configuration values. If there are any left over, they are unrecognized.
			recognizedAttributes.ForEach(config.Remove);

			if (config.Count > 0)
			{
				var unrecognizedAttribute = config.GetKey(0);

				if (!string.IsNullOrEmpty(unrecognizedAttribute))
				{
					throw new ProviderException("The search provider {0} does not currently recognize or support the attribute {1}.".FormatWith(name, unrecognizedAttribute));
				}
			}

			try
			{
				var serviceRoleName = RoleEnvironment.GetConfigurationSettingValue("Adxstudio.Xrm.Search.WindowsAzure.ServiceRole");

				if (string.IsNullOrEmpty(serviceRoleName))
				{
					throw new ProviderException("Configuration value Adxstudio.Xrm.Search.WindowsAzure.ServiceRole cannot be null or empty.");
				}

				Role serviceRole;

				if (!RoleEnvironment.Roles.TryGetValue(serviceRoleName, out serviceRole))
				{
					throw new ProviderException("Unable to retrieve the role {0}.".FormatWith(serviceRoleName));
				}

				var serviceEndpointName = RoleEnvironment.GetConfigurationSettingValue("Adxstudio.Xrm.Search.WindowsAzure.ServiceEndpoint");

				if (string.IsNullOrEmpty(serviceEndpointName))
				{
					throw new ProviderException("Configuration value Adxstudio.Xrm.Search.WindowsAzure.ServiceEndpoint cannot be null or empty.");
				}

				var serviceEndpoint = serviceRole.Instances.Select(instance =>
				{
					RoleInstanceEndpoint endpoint;

					return instance.InstanceEndpoints.TryGetValue(serviceEndpointName, out endpoint) ? endpoint : null;
				}).FirstOrDefault(endpoint => endpoint != null);

				if (serviceEndpoint == null)
				{
					throw new ProviderException("Unable to retrieve the endpoint {0} from role {1}.".FormatWith(serviceEndpointName, serviceRole.Name));
				}

				ServiceEndpointAddress = new EndpointAddress(string.Format(CultureInfo.InvariantCulture, "net.tcp://{0}/search", serviceEndpoint.IPEndpoint));

				var binding = string.IsNullOrEmpty(BindingConfiguration)
					? new NetTcpBinding(SecurityMode.None) { ReceiveTimeout = TimeSpan.FromDays(1), SendTimeout = TimeSpan.FromDays(1) }
					: new NetTcpBinding(BindingConfiguration);

				ServiceChannelFactory = new ChannelFactory<ISearchService>(binding);

				ServiceChannelFactory.Faulted += OnServiceChannelFactoryFaulted;
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format(@"Error initializing provider ""{0}"": {1}", name, e.ToString()));

                throw;
			}
		}

		private void OnServiceChannelFactoryFaulted(object sender, EventArgs e)
		{
            ADXTrace.Instance.TraceError(TraceCategory.Application, "Service channel factory faulted.");
        }

		public override ICrmEntityIndexBuilder GetIndexBuilder()
		{
			return UsingService(service => new IndexBuilder(service));
		}

		public override ICrmEntityIndexSearcher GetIndexSearcher()
		{
			return UsingService(service => new IndexSearcher(service, PortalName));
		}

		public override ICrmEntityIndexUpdater GetIndexUpdater()
		{
			return UsingService(service => new IndexUpdater(service));
		}

		public override IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo()
		{
			return GetIndexedEntityInfo(null);
		}

		public override IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo(int languageCode)
		{
			return GetIndexedEntityInfo(languageCode);
		}

		private IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo(int? languageCode)
		{
			var info = UsingService(service => service.GetIndexedEntityInfo(), true);

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var metadata = info.LogicalNames.Select(logicalName =>
			{
				var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = logicalName, EntityFilters = EntityFilters.Entity
				});

				return response.EntityMetadata;
			});

			return metadata
				.Select(m => new CrmEntityIndexInfo(m.LogicalName, GetEntityDisplayName(m, languageCode), GetEntityDisplayCollectionName(m, languageCode)))
				.ToList();
		}

		protected TResult UsingService<TResult>(Func<ISearchService, TResult> action, bool close = false)
		{
			try
			{
				var channel = ServiceChannelFactory.CreateChannel(ServiceEndpointAddress);

				try
				{
					return action(channel);
				}
				finally
				{
					if (close)
					{
						try
						{
							((IClientChannel)channel).Close();
						}
						catch
						{
							((IClientChannel)channel).Abort();
						}
					}
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                throw;
			}
		}

		private static string GetEntityDisplayCollectionName(EntityMetadata metadata, int? languageCode)
		{
			if (languageCode == null)
			{
				return metadata.DisplayCollectionName.GetLocalizedLabelString();
			}

			var label = metadata.DisplayCollectionName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode.Value);

			return label != null ? label.Label : null;
		}

		private static string GetEntityDisplayName(EntityMetadata metadata, int? languageCode)
		{
			if (languageCode == null)
			{
				return metadata.DisplayName.GetLocalizedLabelString();
			}

			var label = metadata.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode.Value);

			return label != null ? label.Label : null;
		}

		protected class IndexBuilder : ICrmEntityIndexBuilder
		{
			private readonly ISearchService _service;

			public IndexBuilder(ISearchService service)
			{
				if (service == null)
				{
					throw new ArgumentNullException("service");
				}

				_service = service;
			}

			public void BuildIndex()
			{
				try
				{
					_service.BuildIndex();
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                    throw;
				}
			}

			public void Dispose()
			{
				try
				{
					((IClientChannel)_service).Close();
				}
				catch
				{
					((IClientChannel)_service).Abort();
				}
			}
		}

		protected class IndexSearcher : ICrmEntityIndexSearcher
		{
			private readonly string _portalName;
			private readonly ISearchService _service;

			public IndexSearcher(ISearchService service, string portalName)
			{
				if (service == null)
				{
					throw new ArgumentNullException("service");
				}

				_service = service;
				_portalName = portalName;
			}

			public void Dispose()
			{
				try
				{
					((IClientChannel)_service).Close();
				}
				catch
				{
					((IClientChannel)_service).Abort();
				}
			}

			public ICrmEntitySearchResultPage Search(ICrmEntityQuery query)
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(_portalName);
				var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(_portalName);
				var dependencyProvider = PortalCrmConfigurationManager.CreateDependencyProvider(_portalName);
				var security = GetSecurityAssertion(_portalName);

				var metadataCache = new Dictionary<string, EntityMetadata>();

				var paginator = new TopPaginator<ICrmEntitySearchResult>(
					query.PageSize,
					top => GetTopSearchResults(top, query, serviceContext, portal, security, dependencyProvider, entityName => GetEntityMetadata(serviceContext, entityName, metadataCache)),
					result => result != null);

				var results = paginator.GetPage(query.PageNumber);

				return new CrmEntitySearchResultPage(results, results.TotalUnfilteredItems, query.PageNumber, query.PageSize);
			}

			private Func<OrganizationServiceContext, Entity, bool> GetSecurityAssertion(string portalName)
			{
				var cmsSecurityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);

				if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
				{
					return (serviceContext, entity) => cmsSecurityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read);
				}

				var entityPermissionProvider = new CrmEntityPermissionProvider(portalName);

				return (serviceContext, entity) =>
				{
					if (cmsSecurityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
					{
						return true;
					}

					var permissionResult = entityPermissionProvider.TryAssert(serviceContext, entity);

					return permissionResult.RulesExist && permissionResult.CanRead;
				};
			}

			private TopPaginator<ICrmEntitySearchResult>.Top GetTopSearchResults(int top, ICrmEntityQuery query, OrganizationServiceContext serviceContext, IPortalContext portal, Func<OrganizationServiceContext, Entity, bool> assertSecurity, IDependencyProvider dependencyProvider, Func<string, EntityMetadata> getEntityMetadata)
			{
				EntitySearchResultPage searchResponse;

				try
				{
					searchResponse = _service.Search(query.QueryText, 1, top, string.Join(",", query.LogicalNames.ToArray()), portal.Website.Id.ToString(), query.Filter);
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                    throw;
				}

				if (searchResponse.IndexNotFound)
				{
					throw new IndexNotFoundException("Search index not found. Please ensure that the search index is constructed before attempting a query.");
				}

				var currentResultNumber = 0;

				var items = searchResponse.Results.Select(result =>
				{
					var metadata = getEntityMetadata(result.EntityLogicalName);

					if (metadata == null)
					{
						return null;
					}

					var entity = serviceContext.CreateQuery(metadata.LogicalName)
						.FirstOrDefault(e => e.GetAttributeValue<Guid>(metadata.PrimaryIdAttribute) == result.EntityID);

					if (entity == null)
					{
						return null;
					}

					if (!assertSecurity(serviceContext, entity))
					{
						return null;
					}

					var urlProvider = dependencyProvider.GetDependency<IEntityUrlProvider>();

					var path = urlProvider.GetUrl(serviceContext, entity);

					if (path == null)
					{
						return null;
					}

					Uri url;

					if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out url))
					{
						return null;
					}

					currentResultNumber++;

					return new CrmEntitySearchResult(entity, result.Score, currentResultNumber, result.Title, url)
					{
						Fragment = result.Fragment
					};
				});

				return new TopPaginator<ICrmEntitySearchResult>.Top(items, searchResponse.ApproximateTotalHits);
			}

			private static EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext, string logicalName, IDictionary<string, EntityMetadata> cache)
			{
				EntityMetadata metadata;

				if (cache.TryGetValue(logicalName, out metadata))
				{
					return metadata;
				}

				var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = logicalName, EntityFilters = EntityFilters.Entity
				});

				cache[logicalName] = response.EntityMetadata;

				return response.EntityMetadata;
			}
		}

		protected class IndexUpdater : ICrmEntityIndexUpdater
		{
			private readonly ISearchService _service;

			public IndexUpdater(ISearchService service)
			{
				if (service == null)
				{
					throw new ArgumentNullException("service");
				}

				_service = service;
			}

			public void DeleteEntity(string entityLogicalName, Guid id)
			{
				try
				{
					_service.DeleteEntity(entityLogicalName, id);
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                    throw;
				}
			}

			public void DeleteEntitySet(string entityLogicalName)
			{
				try
				{
					_service.DeleteEntitySet(entityLogicalName);
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                    throw;
				}
			}

			public void UpdateEntity(string entityLogicalName, Guid id)
			{
				try
				{
					_service.UpdateEntity(entityLogicalName, id);
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                    throw;
				}
			}

			public void UpdateEntitySet(string entityLogicalName)
			{
				try
				{
					_service.UpdateEntitySet(entityLogicalName);
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Service exception: {0}", e.ToString()));

                    throw;
				}
			}

			public void UpdateEntitySet(string entityLogicalName, string entityAttribute, List<Guid> entityIds)
			{
				return;
			}

			public void UpdateCmsEntityTree(string entityLogicalName, Guid rootEntityId, int? lcid = null)
            {
                return;
            }

			public void Dispose()
			{
				try
				{
					((IClientChannel)_service).Close();
				}
				catch
				{
					((IClientChannel)_service).Abort();
				}
			}
		}
	}
}
