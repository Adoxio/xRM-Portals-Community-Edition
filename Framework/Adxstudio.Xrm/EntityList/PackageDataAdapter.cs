/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.EntityList
{
	public class PackageDataAdapter
	{
		public PackageDataAdapter(EntityReference package, IDataAdapterDependencies dependencies, Func<Guid, Guid, string> getPackageRepositoryUrl, Func<Guid, Guid, string> getPackageVersionUrl, Func<Guid, Guid, string> getPackageImageUrl)
		{
			if (package == null) throw new ArgumentNullException("package");
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (getPackageRepositoryUrl == null) throw new ArgumentNullException("getPackageRepositoryUrl");
			if (getPackageVersionUrl == null) throw new ArgumentNullException("getPackageVersionUrl");
			if (getPackageImageUrl == null) throw new ArgumentNullException("getPackageImageUrl");

			Package = package;
			Dependencies = dependencies;
			GetPackageRepositoryUrl = getPackageRepositoryUrl;
			GetPackageVersionUrl = getPackageVersionUrl;
			GetPackageImageUrl = getPackageImageUrl;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }
		
		protected Func<Guid, Guid, string> GetPackageImageUrl { get; private set; }

		protected Func<Guid, Guid, string> GetPackageRepositoryUrl { get; private set; }

		protected Func<Guid, Guid, string> GetPackageVersionUrl { get; private set; }

		protected EntityReference Package { get; private set; }

		public Package SelectPackage()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var fetch = new Fetch
			{
				Version = "1.0",
				MappingType = MappingType.Logical,
				Entity = new FetchEntity
				{
					Name = Package.LogicalName,
					Attributes = FetchAttribute.All,
					Filters = new[]
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new[]
							{
								new Condition("adx_packageid", ConditionOperator.Equal, Package.Id),
								new Condition("statecode", ConditionOperator.Equal, 0),
							}
						}
					},
					Links = new Collection<Link>()
				}
			};

			AddPackageCategoryJoin(fetch.Entity);
			AddPackageComponentJoin(fetch.Entity);
			AddPackageDependencyJoin(fetch.Entity);
			AddPackageImageJoin(fetch.Entity);
			AddPackagePublisherJoin(fetch.Entity);
			AddPackageVersionJoin(fetch.Entity);

			var entityGrouping = FetchEntities(serviceContext, fetch)
				.GroupBy(e => e.Id)
				.FirstOrDefault();

			if (entityGrouping == null)
			{
				return null;
			}

			var entity = entityGrouping.FirstOrDefault();

			if (entity == null)
			{
				return null;
			}

			var versions = GetPackageVersions(entityGrouping, website.Id)
				.OrderByDescending(e => e.ReleaseDate)
				.ToArray();

			var currentVersion = versions.FirstOrDefault();

			if (currentVersion == null)
			{
				return null;
			}

			PackageImage icon;

			var images = GetPackageImages(entityGrouping, website.Id, entity.GetAttributeValue<EntityReference>("adx_iconid"), out icon)
				.OrderBy(e => e.Name)
				.ToArray();

			var packageRepository = entity.GetAttributeValue<EntityReference>("adx_packagerepository");

			return new Package
			{
				Categories = GetPackageCategories(entityGrouping).ToArray(),
				Components = GetPackageComponents(entityGrouping, website, packageRepository).OrderBy(e => e.Order).ThenBy(e => e.CreatedOn).ToArray(),
				ContentUrl = currentVersion.Url,
				Dependencies = GetPackageDependencies(entityGrouping, website, packageRepository).OrderBy(e => e.Order).ThenBy(e => e.CreatedOn).ToArray(),
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
				Uri = GetPackageUri(packageRepository, website.Id, entity.GetAttributeValue<string>("adx_uniquename")),
				Url = null,
				Version = currentVersion.Version,
				Versions = versions
			};
		}

		protected virtual IEnumerable<Entity> FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch)
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

		private IEnumerable<PackageComponent> GetPackageComponents(IGrouping<Guid, Entity> entityGrouping, EntityReference website, EntityReference packageRepository)
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
					Uri = GetPackageUri(componentPackageRepository, website.Id, packageUniqueName),
					Version = entity.GetAttributeAliasedValue<string>("adx_version", "component"),
					Order = entity.GetAttributeAliasedValue<int?>("adx_order", "component").GetValueOrDefault(0),
					CreatedOn = entity.GetAttributeAliasedValue<DateTime?>("createdon", "component").GetValueOrDefault()
				};
			}
		}

		private IEnumerable<PackageDependency> GetPackageDependencies(IGrouping<Guid, Entity> entityGrouping, EntityReference website, EntityReference packageRepository)
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
						Uri = GetPackageUri(dependencyPackageRepository, website.Id, packageUniqueName),
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

				yield return new PackageVersion
				{
					Description = entity.GetAttributeAliasedValue<string>("adx_description", "version"),
					DisplayName = entity.GetAttributeAliasedValue<string>("adx_name", "version"),
					ReleaseDate = entity.GetAttributeAliasedValue<DateTime?>("adx_releasedate", "version").GetValueOrDefault(),
					RequiredInstallerVersion = entity.GetAttributeAliasedValue<string>("adx_requiredinstallerversion", "version"),
					Url = GetPackageVersionUrl(websiteId, versionId.Value),
					Version = entity.GetAttributeAliasedValue<string>("adx_version", "version"),
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
					new FetchAttribute("adx_version"),
					new FetchAttribute("adx_requiredinstallerversion")
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
}
