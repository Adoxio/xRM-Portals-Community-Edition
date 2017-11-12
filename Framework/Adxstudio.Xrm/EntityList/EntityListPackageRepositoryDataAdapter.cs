/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.EntityList
{
	public class EntityListPackageRepositoryDataAdapter : EntityListDataAdapter
	{
		public EntityListPackageRepositoryDataAdapter(EntityReference packageRepository, EntityReference entityList, EntityReference view, IDataAdapterDependencies dependencies, Func<Guid, Guid, string> getPackageRepositoryUrl, Func<Guid, Guid, string> getPackageVersionUrl, Func<Guid, Guid, string> getPackageImageUrl)
			: base(entityList, view, dependencies)
		{
			if (packageRepository == null) throw new ArgumentNullException("packageRepository");
			if (getPackageRepositoryUrl == null) throw new ArgumentNullException("getPackageRepositoryUrl");
			if (getPackageVersionUrl == null) throw new ArgumentNullException("getPackageVersionUrl");
			if (getPackageImageUrl == null) throw new ArgumentNullException("getPackageImageUrl");

			PackageRepository = packageRepository;
			GetPackageRepositoryUrl = getPackageRepositoryUrl;
			GetPackageVersionUrl = getPackageVersionUrl;
			GetPackageImageUrl = getPackageImageUrl;
		}

		protected Func<Guid, Guid, string> GetPackageImageUrl { get; private set; }

		protected Func<Guid, Guid, string> GetPackageRepositoryUrl { get; private set; }

		protected Func<Guid, Guid, string> GetPackageVersionUrl { get; private set; }

		protected EntityReference PackageRepository { get; private set; }
		

		public PackageRepository SelectRepository(string category = null, string filter = null, string search = null)
		{
			AssertEntityListAccess();

			var serviceContext = Dependencies.GetServiceContext();

			Entity packageRepository;

			if (!TryGetPackageRepository(serviceContext, PackageRepository, out packageRepository))
			{
                throw new InvalidOperationException("Unable to retrieve the package repository ({0}:{1}).".FormatWith(PackageRepository.LogicalName, PackageRepository.Id));
			}

			Entity entityList;

			if (!TryGetEntityList(serviceContext, EntityList, out entityList))
			{
				throw new InvalidOperationException("Unable to retrieve the entity list ({0}:{1}).".FormatWith(EntityList.LogicalName, EntityList.Id));
			}

			Entity view;

			if (!TryGetView(serviceContext, entityList, View, out view))
			{
				throw new InvalidOperationException("Unable to retrieve view ({0}:{1}).".FormatWith(View.LogicalName, View.Id));
			}

			var fetchXml = view.GetAttributeValue<string>("fetchxml");

			if (string.IsNullOrEmpty(fetchXml))
			{
				throw new InvalidOperationException("Unable to retrieve the view FetchXML ({0}:{1}).".FormatWith(View.LogicalName, View.Id));
			}

			var fetch = Fetch.Parse(fetchXml);

			var searchableAttributes = fetch.Entity.Attributes.Select(a => a.Name).ToArray();

			fetch.Entity.Attributes = FetchAttribute.All;

			var settings = new EntityListSettings(entityList);

			AddPackageCategoryJoin(fetch.Entity);
			AddPackageComponentJoin(fetch.Entity);
			AddPackageDependencyJoin(fetch.Entity);
			AddPackageImageJoin(fetch.Entity);
			AddPackagePublisherJoin(fetch.Entity);
			AddPackageVersionJoin(fetch.Entity);

			// Refactor the query to reduce links, but just do an in-memory category filter for now.
			//AddPackageCategoryFilter(fetch.Entity, category);

			AddSelectableFilterToFetchEntity(fetch.Entity, settings, filter);
			AddWebsiteFilterToFetchEntity(fetch.Entity, settings);
			AddSearchFilterToFetchEntity(fetch.Entity, settings, search, searchableAttributes);

			var entityGroupings = FetchEntities(serviceContext, fetch).GroupBy(e => e.Id);

			var packages = GetPackages(entityGroupings, GetPackageUrl(settings)).ToArray();

			var categoryComparer = new PackageCategoryComparer();

			var repositoryCategories = packages.SelectMany(e => e.Categories).Distinct(categoryComparer).OrderBy(e => e.Name);

			// Do in-memory category filter.
			if (!string.IsNullOrWhiteSpace(category))
			{
				var filterCategory = new PackageCategory(category);

				packages = packages
					.Where(e => e.Categories.Any(c => categoryComparer.Equals(c, filterCategory)))
					.ToArray();
			}

			return new PackageRepository
			{
				Title = packageRepository.GetAttributeValue<string>("adx_name"),
				Categories = repositoryCategories,
				Packages = packages,
				Description = packageRepository.GetAttributeValue<string>("adx_description"),
				RequiredInstallerVersion = packageRepository.GetAttributeValue<string>("adx_requiredinstallerversion")
			};
		}

		protected override IEnumerable<Entity> FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch)
		{
			fetch.PageNumber = 1;

			while (true)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

				foreach (var entity in response.EntityCollection.Entities)
				{
					yield return entity;
				}

				if (!response.EntityCollection.MoreRecords)
				{
					break;
				}

				fetch.PageNumber++;
			}
		}

		private Func<Guid, string> GetPackageUrl(EntityListSettings settings)
		{
			if (settings.WebPageForDetailsView == null)
			{
				return id => null;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var page = serviceContext.CreateQuery("adx_webpage")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webpageid") == settings.WebPageForDetailsView.Id);

			if (page == null)
			{
				return id => null;
			}

			var pageUrl = Dependencies.GetUrlProvider().GetUrl(serviceContext, page);

			if (string.IsNullOrEmpty(pageUrl))
			{
				return id => null;
			}

			if (string.IsNullOrWhiteSpace(settings.IdQueryStringParameterName))
			{
				return id => new UrlBuilder(pageUrl) { Fragment = id.ToString() }.ToString();
			}

			return id =>
			{
				var url = new UrlBuilder(pageUrl);

				url.QueryString.Set(settings.IdQueryStringParameterName, id.ToString());

				return url.ToString();
			};
		}

		private IEnumerable<Package> GetPackages(IEnumerable<IGrouping<Guid, Entity>> entityGroupings, Func<Guid, string> getPackageUrl)
		{
			var website = Dependencies.GetWebsite();

			foreach (var entityGrouping in entityGroupings)
			{
				var entity = entityGrouping.FirstOrDefault();

				if (entity == null)
				{
					continue;
				}

				var versions = GetPackageVersions(entityGrouping, website.Id)
					.OrderByDescending(e => e.ReleaseDate)
					.ToArray();

				var currentVersion = versions.FirstOrDefault();

				if (currentVersion == null)
				{
					continue;
				}

				PackageImage icon;

				var images = GetPackageImages(entityGrouping, website.Id, entity.GetAttributeValue<EntityReference>("adx_iconid"), out icon)
					.OrderBy(e => e.Name)
					.ToArray();

				yield return new Package
				{
					Categories = GetPackageCategories(entityGrouping).ToArray(),
					Components = GetPackageComponents(entityGrouping, website.Id).OrderBy(e => e.Order).ThenBy(e => e.CreatedOn).ToArray(),
					ContentUrl = currentVersion.Url,
					Dependencies = GetPackageDependencies(entityGrouping, website.Id).OrderBy(e => e.Order).ThenBy(e => e.CreatedOn).ToArray(),
					Description = entity.GetAttributeValue<string>("adx_description"),
					DisplayName = entity.GetAttributeValue<string>("adx_name"),
					HideFromPackageListing = entity.GetAttributeValue<bool?>("adx_hidefromlisting").GetValueOrDefault(false),
					Icon = icon,
					Images = images,
					OverwriteWarning = entity.GetAttributeValue<bool?>("adx_overwritewarning").GetValueOrDefault(false),
					PublisherName = entity.GetAttributeAliasedValue<string>("adx_name", "publisher"),
					ReleaseDate = currentVersion.ReleaseDate,
					RequiredInstallerVersion = currentVersion.RequiredInstallerVersion,
					Summary = entity.GetAttributeValue<string>("adx_summary"),
					Type = GetPackageType(entity.GetAttributeValue<OptionSetValue>("adx_type")),
					UniqueName = entity.GetAttributeValue<string>("adx_uniquename"),
					Uri = GetPackageUri(entity.GetAttributeValue<EntityReference>("adx_packagerepositoryid"), website.Id, entity.GetAttributeValue<string>("adx_uniquename")),
					Url = getPackageUrl(entity.Id),
					Version = currentVersion.Version,
					Versions = versions,
					Configuration = currentVersion.Configuration
				};
			}
		}

		private IEnumerable<PackageCategory> GetPackageCategories(IGrouping<Guid, Entity> entityGrouping)
		{
			return entityGrouping
				.GroupBy(e => e.GetAttributeAliasedValue<Guid?>("adx_packagecategoryid", "category"))
				.Where(e => e.Key != null)
				.Select(e => e.FirstOrDefault())
				.Where(e => e != null)
				.Select(e => e.GetAttributeAliasedValue<string>("adx_name", "category"))
				.Where(e => !string.IsNullOrWhiteSpace(e))
				.Select(e => new PackageCategory(e))
				.Distinct(new PackageCategoryComparer())
				.OrderBy(e => e.Name);
		}

		private IEnumerable<PackageComponent> GetPackageComponents(IGrouping<Guid, Entity> entityGrouping, Guid websiteId)
		{
			var groupings = entityGrouping
				.GroupBy(e => e.GetAttributeAliasedValue<Guid?>("adx_packagecomponentid", "component"))
				.Where(e => e.Key != null);

			foreach (var grouping in groupings)
			{
				var entity = grouping.FirstOrDefault();

				if (entity == null)
				{
					continue;
				}

				var packageDisplayName = entity.GetAttributeAliasedValue<string>("adx_name", "componentpackage");

				if (string.IsNullOrWhiteSpace(packageDisplayName))
				{
					continue;
				}

				var packageUniqueName = entity.GetAttributeAliasedValue<string>("adx_uniquename", "componentpackage");

				if (string.IsNullOrWhiteSpace(packageUniqueName))
				{
					continue;
				}

				var componentPackageRepository = entity.GetAttributeAliasedValue<EntityReference>("adx_packagerepositoryid", "componentpackage");

				yield return new PackageComponent
				{
					DisplayName = packageDisplayName,
					Uri = GetPackageUri(componentPackageRepository, websiteId, packageUniqueName),
					Version = entity.GetAttributeAliasedValue<string>("adx_version", "component"),
					Order = entity.GetAttributeAliasedValue<int?>("adx_order", "component").GetValueOrDefault(0),
					CreatedOn = entity.GetAttributeAliasedValue<DateTime?>("createdon", "component").GetValueOrDefault()
				};
			}
		}

		private IEnumerable<PackageDependency> GetPackageDependencies(IGrouping<Guid, Entity> entityGrouping, Guid websiteId)
		{
			var groupings = entityGrouping
				.GroupBy(e => e.GetAttributeAliasedValue<Guid?>("adx_packagedependencyid", "dependency"))
				.Where(e => e.Key != null);

			foreach (var grouping in groupings)
			{
				var entity = grouping.FirstOrDefault();

				if (entity == null)
				{
					continue;
				}

				var packageDisplayName = entity.GetAttributeAliasedValue<string>("adx_name", "dependencypackage");
				var packageUniqueName = entity.GetAttributeAliasedValue<string>("adx_uniquename", "dependencypackage");

				if (!string.IsNullOrWhiteSpace(packageDisplayName) && !string.IsNullOrWhiteSpace(packageUniqueName))
				{
					var dependencyPackageRepository = entity.GetAttributeAliasedValue<EntityReference>("adx_packagerepositoryid", "dependencypackage");

					yield return new PackageDependency
					{
						DisplayName = packageDisplayName,
						Uri = GetPackageUri(dependencyPackageRepository, websiteId, packageUniqueName),
						Version = entity.GetAttributeAliasedValue<string>("adx_version", "dependency"),
						Order = entity.GetAttributeAliasedValue<int?>("adx_order", "dependency").GetValueOrDefault(0),
						CreatedOn = entity.GetAttributeAliasedValue<DateTime?>("createdon", "dependency").GetValueOrDefault()
					};

					continue;
				}

				var packageUri = entity.GetAttributeAliasedValue<string>("adx_dependencypackageuri", "dependency");
				Uri parsed;

				if (!string.IsNullOrWhiteSpace(packageUri) && Uri.TryCreate(packageUri, UriKind.RelativeOrAbsolute, out parsed))
				{
					yield return new PackageDependency
					{
						DisplayName = entity.GetAttributeAliasedValue<string>("adx_name", "dependency"),
						Uri = parsed,
						Version = entity.GetAttributeAliasedValue<string>("adx_version", "dependency"),
						Order = entity.GetAttributeAliasedValue<int?>("adx_order", "dependency").GetValueOrDefault(0),
						CreatedOn = entity.GetAttributeAliasedValue<DateTime?>("createdon", "dependency").GetValueOrDefault()
					};
				}
			}
		}

		private Uri GetPackageUri(EntityReference packageRepository, Guid websiteId, string packageUniqueName)
		{
			if (packageRepository == null)
			{
				return new Uri("#" + packageUniqueName, UriKind.Relative);
			}

			var repositoryUrl = GetPackageRepositoryUrl(websiteId, packageRepository.Id);

			return repositoryUrl == null
				? new Uri("#" + packageUniqueName, UriKind.Relative)
				: new Uri(repositoryUrl + "#" + packageUniqueName, UriKind.RelativeOrAbsolute);
		}

		private IEnumerable<PackageImage> GetPackageImages(IGrouping<Guid, Entity> entityGrouping, Guid websiteId, EntityReference iconReference, out PackageImage icon)
		{
			icon = null;
			var images = new List<PackageImage>();

			var groupings = entityGrouping
				.GroupBy(e => e.GetAttributeAliasedValue<Guid?>("adx_packageimageid", "image"))
				.Where(e => e.Key != null);

			foreach (var grouping in groupings)
			{
				var entity = grouping.FirstOrDefault();

				if (entity == null)
				{
					continue;
				}

				var imageId = entity.GetAttributeAliasedValue<Guid?>("adx_packageimageid", "image");

				if (imageId == null)
				{
					continue;
				}

				var image = new PackageImage
				{
					Name = entity.GetAttributeAliasedValue<string>("adx_name", "image"),
					Description = entity.GetAttributeAliasedValue<string>("adx_description", "image")
						?? entity.GetAttributeAliasedValue<string>("adx_name", "image"),
					Url = GetPackageImageUrl(websiteId, imageId.Value)
				};

				if (iconReference != null && iconReference.Id == imageId)
				{
					icon = image;
				}
				else
				{
					images.Add(image);
				}
			}

			return images;
		}

		private PackageType GetPackageType(OptionSetValue type)
		{
			if (type == null)
			{
				return PackageType.Solution;
			}

			if (type.Value == (int)PackageType.Data)
			{
				return PackageType.Data;
			}

			return PackageType.Solution;
		}

		private IEnumerable<PackageVersion> GetPackageVersions(IGrouping<Guid, Entity> entityGrouping, Guid websiteId)
		{
			var groupings = entityGrouping
				.GroupBy(e => e.GetAttributeAliasedValue<Guid?>("adx_packageversionid", "version"))
				.Where(e => e.Key != null);

			foreach (var grouping in groupings)
			{
				var entity = grouping.FirstOrDefault();

				if (entity == null)
				{
					continue;
				}

				var versionId = entity.GetAttributeAliasedValue<Guid?>("adx_packageversionid", "version");

				if (versionId == null)
				{
					continue;
				}

				var configurationJson = entity.GetAttributeAliasedValue<string>("adx_configuration", "version");
				var configuration = new PackageConfiguration();
				if (!string.IsNullOrWhiteSpace(configurationJson))
				{
					try
					{
						configuration = JsonConvert.DeserializeObject<PackageConfiguration>(configurationJson,
							new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, Binder = new PackageConfigurationSerializationBinder() });
					}
					catch (Exception e)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("GetPackageVersions", "{0}", e.ToString()));
                    }
				}

				yield return new PackageVersion
				{
					Description = entity.GetAttributeAliasedValue<string>("adx_description", "version"),
					DisplayName = entity.GetAttributeAliasedValue<string>("adx_name", "version"),
					ReleaseDate = entity.GetAttributeAliasedValue<DateTime?>("adx_releasedate", "version").GetValueOrDefault(),
					RequiredInstallerVersion = entity.GetAttributeAliasedValue<string>("adx_requiredinstallerversion", "version"),
					Url = GetPackageVersionUrl(websiteId, versionId.Value),
					Version = entity.GetAttributeAliasedValue<string>("adx_version", "version"),
					Configuration = configuration
				};
			}
		}

		protected virtual void AddPackageCategoryJoin(FetchEntity fetchEntity)
		{
			fetchEntity.Links.Add(new Link
			{
				Name = "adx_package_packagecategory",
				FromAttribute = "adx_packageid",
				ToAttribute = "adx_packageid",
				Type = JoinOperator.LeftOuter,
				Links = new[]
				{
					new Link
					{
						Alias = "category",
						Name = "adx_packagecategory",
						FromAttribute = "adx_packagecategoryid",
						ToAttribute = "adx_packagecategoryid",
						Type = JoinOperator.LeftOuter,
						Attributes = new[]
						{
							new FetchAttribute("adx_packagecategoryid"),
							new FetchAttribute("adx_name"),
						},
						Filters = new[]
						{
							new Filter
							{
								Conditions = new[]
								{
									new Condition("statecode", ConditionOperator.Equal, 0)
								}
							}
						}
					}
				}
			});
		}

		protected virtual void AddPackageComponentJoin(FetchEntity fetchEntity)
		{
			fetchEntity.Links.Add(new Link
			{
				Alias = "component",
				Name = "adx_packagecomponent",
				FromAttribute = "adx_packageid",
				ToAttribute = "adx_packageid",
				Type = JoinOperator.LeftOuter,
				Attributes = new[]
				{
					new FetchAttribute("adx_packagecomponentid"),
					new FetchAttribute("adx_name"),
					new FetchAttribute("adx_packageid"),
					new FetchAttribute("adx_componentpackageid"),
					new FetchAttribute("adx_version"),
					new FetchAttribute("adx_order"),
					new FetchAttribute("createdon"),
				},
				Links = new[]
				{
					new Link
					{
						Alias = "componentpackage",
						Name = "adx_package",
						FromAttribute = "adx_packageid",
						ToAttribute = "adx_componentpackageid",
						Type = JoinOperator.LeftOuter,
						Attributes = new[]
						{
							new FetchAttribute("adx_packageid"),
							new FetchAttribute("adx_name"),
							new FetchAttribute("adx_uniquename"),
							new FetchAttribute("adx_packagerepositoryid")
						},
						Filters = new[]
						{
							new Filter
							{
								Conditions = new[]
								{
									new Condition("statecode", ConditionOperator.Equal, 0)
								}
							}
						}
					}
				},
				Filters = new[]
				{
					new Filter
					{
						Conditions = new[]
						{
							new Condition("statecode", ConditionOperator.Equal, 0)
						}
					}
				}
			});
		}

		protected virtual void AddPackageDependencyJoin(FetchEntity fetchEntity)
		{
			fetchEntity.Links.Add(new Link
			{
				Alias = "dependency",
				Name = "adx_packagedependency",
				FromAttribute = "adx_packageid",
				ToAttribute = "adx_packageid",
				Type = JoinOperator.LeftOuter,
				Attributes = new[]
				{
					new FetchAttribute("adx_packagedependencyid"),
					new FetchAttribute("adx_name"),
					new FetchAttribute("adx_packageid"),
					new FetchAttribute("adx_dependencypackageid"),
					new FetchAttribute("adx_dependencypackageuri"),
					new FetchAttribute("adx_version"),
					new FetchAttribute("adx_order"),
					new FetchAttribute("createdon"),
				},
				Links = new[]
				{
					new Link
					{
						Alias = "dependencypackage",
						Name = "adx_package",
						FromAttribute = "adx_packageid",
						ToAttribute = "adx_dependencypackageid",
						Type = JoinOperator.LeftOuter,
						Attributes = new[]
						{
							new FetchAttribute("adx_packageid"),
							new FetchAttribute("adx_name"),
							new FetchAttribute("adx_uniquename"),
							new FetchAttribute("adx_packagerepositoryid")
						},
						Filters = new[]
						{
							new Filter
							{
								Conditions = new[]
								{
									new Condition("statecode", ConditionOperator.Equal, 0)
								}
							}
						}
					}
				},
				Filters = new[]
				{
					new Filter
					{
						Conditions = new[]
						{
							new Condition("statecode", ConditionOperator.Equal, 0)
						}
					}
				}
			});
		}

		protected virtual void AddPackageImageJoin(FetchEntity fetchEntity)
		{
			fetchEntity.Links.Add(new Link
			{
				Alias = "image",
				Name = "adx_packageimage",
				FromAttribute = "adx_packageid",
				ToAttribute = "adx_packageid",
				Type = JoinOperator.LeftOuter,
				Attributes = new[]
				{
					new FetchAttribute("adx_packageimageid"),
					new FetchAttribute("adx_name"),
					new FetchAttribute("adx_description"),
				},
				Filters = new[]
				{
					new Filter
					{
						Conditions = new[]
						{
							new Condition("statecode", ConditionOperator.Equal, 0)
						}
					}
				}
			});
		}

		protected virtual void AddPackagePublisherJoin(FetchEntity fetchEntity)
		{
			fetchEntity.Links.Add(new Link
			{
				Alias = "publisher",
				Name = "adx_packagepublisher",
				FromAttribute = "adx_packagepublisherid",
				ToAttribute = "adx_publisherid",
				Type = JoinOperator.LeftOuter,
				Attributes = new[]
				{
					new FetchAttribute("adx_name"),
					new FetchAttribute("adx_uniquename"),
				}
			});
		}

		protected virtual void AddPackageVersionJoin(FetchEntity fetchEntity)
		{
			fetchEntity.Links.Add(new Link
			{
				Alias = "version",
				Name = "adx_packageversion",
				FromAttribute = "adx_packageid",
				ToAttribute = "adx_packageid",
				Type = JoinOperator.LeftOuter,
				Attributes = new[]
				{
					new FetchAttribute("adx_packageversionid"),
					new FetchAttribute("adx_description"),
					new FetchAttribute("adx_name"),
					new FetchAttribute("adx_packageid"),
					new FetchAttribute("adx_releasedate"),
					new FetchAttribute("adx_requiredinstallerversion"), 
					new FetchAttribute("adx_version"),
					new FetchAttribute("adx_configuration")
				},
				Filters = new[]
				{
					new Filter
					{
						Conditions = new[]
						{
							new Condition("statecode", ConditionOperator.Equal, 0)
						}
					}
				}
			});
		}

		protected virtual void AddPackageCategoryFilter(FetchEntity fetchEntity, string category)
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				return;
			}

			fetchEntity.Links.Add(new Link
			{
				Name = "adx_package_packagecategory",
				FromAttribute = "adx_packageid",
				ToAttribute = "adx_packageid",
				Type = JoinOperator.Inner,
				Links = new[]
				{
					new Link
					{
						Name = "adx_packagecategory",
						FromAttribute = "adx_packagecategoryid",
						ToAttribute = "adx_packagecategoryid",
						Type = JoinOperator.Inner,
						Filters = new[]
						{
							new Filter
							{
								Type = LogicalOperator.And,
								Conditions = new[]
								{
									new Condition("adx_name", ConditionOperator.Equal, category),
									new Condition("statecode", ConditionOperator.Equal, 0)
								}
							}
						}
					}
				}
			});
		}

		protected static bool TryGetPackageRepository(OrganizationServiceContext serviceContext, EntityReference packageRepositoryReference, out Entity packageRepository)
		{
			 packageRepository = serviceContext.CreateQuery("adx_packagerepository")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_packagerepositoryid") == packageRepositoryReference.Id
					&& e.GetAttributeValue<int?>("statecode") == 0);
			
			return packageRepository != null;
		}

		protected class PackageCategoryComparer : IEqualityComparer<PackageCategory>
		{
			public bool Equals(PackageCategory x, PackageCategory y)
			{
				return StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, y.Name);
			}

			public int GetHashCode(PackageCategory category)
			{
				return category.Name.GetHashCode();
			}
		}
	}

	public class PackageRepository
	{
		public IEnumerable<PackageCategory> Categories { get; set; }

		public IEnumerable<Package> Packages { get; set; }

		public string RequiredInstallerVersion { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }
	}

	public enum PackageType
	{
		Solution = 756150000,
		Data = 756150001
	}

	public class Package
	{
		public IEnumerable<PackageCategory> Categories { get; set; }

		public IEnumerable<PackageComponent> Components { get; set; }

		public PackageConfiguration Configuration { get; set; }

		public string ContentUrl { get; set; }
		
		public IEnumerable<PackageDependency> Dependencies { get; set; }

		public string Description { get; set; }

		public string DisplayName { get; set; }

		public bool HideFromPackageListing { get; set; }

		public PackageImage Icon { get; set; }

		public IEnumerable<PackageImage> Images { get; set; }

		public bool OverwriteWarning { get; set; }

		public string PublisherName { get; set; }

		public DateTime ReleaseDate { get; set; }

		public string RequiredInstallerVersion { get; set; }

		public string Summary { get; set; }

		public PackageType Type { get; set; }

		public string UniqueName { get; set; }

		public Uri Uri { get; set; }

		public string Url { get; set; }

		public string Version { get; set; }

		public IEnumerable<PackageVersion> Versions { get; set; }
	}

	public class PackageCategory
	{
		public PackageCategory(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value can't be null or whitespace.", "name");

			Name = name;
		}

		public string Name { get; private set; }
	}

	public class PackageComponent
	{
		public DateTime CreatedOn { get; set; }

		public string DisplayName { get; set; }

		public int Order { get; set; }

		public Uri Uri { get; set; }

		public string Version { get; set; }
	}

	public class PackageDependency
	{
		public DateTime CreatedOn { get; set; }

		public string DisplayName { get; set; }

		public int Order { get; set; }

		public Uri Uri { get; set; }

		public string Version { get; set; }
	}

	public class PackageImage
	{
		public string Description { get; set; }

		public string Name { get; set; }

		public string Url { get; set; }
	}

	public class PackageVersion
	{
		public PackageConfiguration Configuration { get; set; }

		public string Description { get; set; }

		public string DisplayName { get; set; }

		public DateTime ReleaseDate { get; set; }

		public string RequiredInstallerVersion { get; set; }

		public string Url { get; set; }

		public string Version { get; set; }
	}

	/// <summary>
	/// Definition of entity reference replacements for a given data package
	/// </summary>
	public class PackageReferenceReplacements
	{
		/// <summary>
		/// Id of the record
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Logical name of the entity
		/// </summary>
		public string LogicalName { get; set; }

		/// <summary>
		/// Name of the record
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Operation that informs the installer how to acquire the replacement entity reference. One of the following values 'SelectWebsite', 'FindVisibleState', 'FindByName', 'FindRootPage'.
		/// </summary>
		public string Operation { get; set; }
	}

	/// <summary>
	/// The definition of attribute value replacements for a given data package.
	/// </summary>
	public class PackageAttributeReplacements
	{
		/// <summary>
		/// The Id of the record.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The Logical name of the entity.
		/// </summary>
		public string EntityLogicalName { get; set; }

		/// <summary>
		/// The Logical name of the attribute.
		/// </summary>
		public string AttributeLogicalName { get; set; }

		/// <summary>
		/// One of the valid HTML input types. Use 'textarea' to display a textarea. Use 'select' to display a select dropdown, combined with setting the Options property.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// An optional block of text to provide additional instructions or to describe the intentions of the input.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The field's label text.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// The Id and name of the HTML element.
		/// </summary>
		public string InputId { get; set; }

		/// <summary>
		/// Indicates whether the field is required or not.
		/// </summary>
		public bool Required { get; set; }

		/// <summary>
		/// The placeholder text assigned to the input.
		/// </summary>
		public string Placeholder { get; set; }

		/// <summary>
		/// The maximum length of the input.
		/// </summary>
		public int? MaxLength { get; set; }

		/// <summary>
		/// The minimum value used when Type is 'number' during validation.
		/// </summary>
		public string Min { get; set; }

		/// <summary>
		/// The maximum value used when Type is 'number' during validation.
		/// </summary>
		public string Max { get; set; }

		/// <summary>
		/// The value of increment applicable when Type is 'number'.
		/// </summary>
		public string Step { get; set; }

		/// <summary>
		/// The default value assigned to the input.
		/// </summary>
		public string DefaultValue { get; set; }

		/// <summary>
		/// The options to be created when Type is 'select'.
		/// </summary>
		public IEnumerable<SelectOption> Options { get; set; }

		/// <summary>
		/// The number or rows applicable when Type is 'textarea'.
		/// </summary>
		public int? Rows { get; set; }

		/// <summary>
		/// The number or columns applicable when Type is 'textarea'.
		/// </summary>
		public int? Cols { get; set; }
	}

	public class SelectOption
	{
		public string Value { get; set; }

		public string Text { get; set; }
	}

	public class PackageConfiguration
	{
		public bool DuplicationEnabled { get; set; }

		public IEnumerable<PackageReferenceReplacements> ReferenceReplacementTargets { get; set; }

		public IEnumerable<PackageAttributeReplacements> AttributeReplacementTargets { get; set; }
	}

	public class PackageConfigurationSerializationBinder : SerializationBinder
	{
		private IList<Type> _knownTypes;

		/// <summary>
		/// 
		/// </summary>
		private IList<Type> KnownTypes
		{
			get
			{
				if (_knownTypes == null)
				{
					_knownTypes = new List<Type>
					{
						typeof(PackageConfiguration)
					};
				}
				return _knownTypes;
			}
		}

		public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			assemblyName = null;
			typeName = serializedType.Name;
		}
		public override Type BindToType(string assemblyName, string typeName)
		{
			return KnownTypes.FirstOrDefault(t => t.FullName == typeName) ?? typeof(NotSupportedAction);
		}
	}
}
