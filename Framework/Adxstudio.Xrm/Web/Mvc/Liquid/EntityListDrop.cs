/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using GridMetadata = Adxstudio.Xrm.Web.UI.JsonConfiguration.GridMetadata;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityListDrop : EntityDrop
	{
		private readonly Lazy<string> _createUrl;
		private readonly Lazy<string> _createLabel;
		private readonly Lazy<string> _detailUrl;
		private readonly Lazy<string> _detailLabel;
		private readonly Lazy<string> _emptyListText;
		private readonly Lazy<string> _filterApplyLabel;
		private readonly Lazy<string> _getDataUrl;
		private readonly Lazy<GridMetadataDrop> _gridMetadata;
		private readonly Lazy<int> _languageCode;
		private readonly Lazy<string> _layouts;
		private readonly Lazy<string> _modalFormTemplateUrl;
		private readonly Lazy<string> _searchPlaceholder;
		private readonly Lazy<string> _searchTooltip;
		private readonly Lazy<EntityListViewDrop[]> _views;

		public EntityListDrop(IPortalLiquidContext portalLiquidContext, Entity entityList, Lazy<string> getDataUrl, Lazy<string> modalFormTemplateUrl, Lazy<string> createUrl, Lazy<string> detailUrl, Lazy<int> languageCode)
			: base(portalLiquidContext, entityList)
		{
			if (getDataUrl == null) throw new ArgumentNullException("getDataUrl");
			if (createUrl == null) throw new ArgumentNullException("createUrl");
			if (detailUrl == null) throw new ArgumentNullException("detailUrl");
			if (languageCode == null) throw new ArgumentNullException("languageCode");
			if (modalFormTemplateUrl == null) throw new ArgumentNullException("modalFormTemplateUrl");

			_createUrl = createUrl;
			_detailUrl = detailUrl;
			_getDataUrl = getDataUrl;
			_languageCode = languageCode;
			_modalFormTemplateUrl = modalFormTemplateUrl;

			EntityLogicalName = entityList.GetAttributeValue<string>("adx_entityname");
			PrimaryKeyName = entityList.GetAttributeValue<string>("adx_primarykeyname");
			DetailIdParameter = entityList.GetAttributeValue<string>("adx_idquerystringparametername");
			EnableEntityPermissions = entityList.GetAttributeValue<bool?>("adx_entitypermissionsenabled").GetValueOrDefault(false);
			FilterPortalUserAttributeName = entityList.GetAttributeValue<string>("adx_filterportaluser");
			FilterAccountAttributeName = entityList.GetAttributeValue<string>("adx_filteraccount");
			FilterWebsiteAttributeName = entityList.GetAttributeValue<string>("adx_filterwebsite");
			FilterEnabled = entityList.GetAttributeValue<bool?>("adx_filter_enabled").GetValueOrDefault(false);
			FilterDefinition = entityList.GetAttributeValue<string>("adx_filter_definition");
			PageSize = entityList.GetAttributeValue<int?>("adx_pagesize");
			SearchEnabled = entityList.GetAttributeValue<bool?>("adx_searchenabled").GetValueOrDefault(false);

			var gridMetadataJson = entityList.GetAttributeValue<string>("adx_settings");

			if (!string.IsNullOrWhiteSpace(gridMetadataJson))
			{
				try
				{
					GridMetadataConfiguration = JsonConvert.DeserializeObject<GridMetadata>(gridMetadataJson,
						new JsonSerializerSettings
						{
							ContractResolver = JsonConfigurationContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Objects,
							Converters = new List<JsonConverter> { new GuidConverter() },
							Binder = new ActionSerializationBinder()
						});
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
				}
			}

			var viewMetadataJson = entityList.GetAttributeValue<string>("adx_views");
			if (!string.IsNullOrWhiteSpace(gridMetadataJson))
			{
				try
				{
					ViewMetadataConfiguration = JsonConvert.DeserializeObject<ViewMetadata>(viewMetadataJson,
						new JsonSerializerSettings
						{
							ContractResolver = JsonConfigurationContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Objects,
							Binder = new ActionSerializationBinder(),
							Converters = new List<JsonConverter> { new GuidConverter() }
						});
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
				}
			}

			var filterOrientation = entityList.GetAttributeValue<OptionSetValue>("adx_filter_orientation");
			if (filterOrientation != null)
			{
				if (Enum.IsDefined(typeof(FilterConfiguration.FilterOrientation), filterOrientation.Value))
				{
					IsFilterVertical = (FilterConfiguration.FilterOrientation)filterOrientation.Value ==
					                   FilterConfiguration.FilterOrientation.Vertical;
				}
			}

			_gridMetadata = new Lazy<GridMetadataDrop>(() => new GridMetadataDrop(portalLiquidContext, GridMetadataConfiguration, LanguageCode));
			_views = new Lazy<EntityListViewDrop[]>(GetViews, LazyThreadSafetyMode.None);
			_layouts = new Lazy<string>(() => GetLayouts(entityList), LazyThreadSafetyMode.None);
			_createLabel = CreateLazyLocalizedString("adx_createbuttonlabel");
			_detailLabel = CreateLazyLocalizedString("adx_detailsbuttonlabel");
			_emptyListText = CreateLazyLocalizedString("adx_emptylisttext");
			_searchPlaceholder = CreateLazyLocalizedString("adx_searchplaceholdertext");
			_searchTooltip = CreateLazyLocalizedString("adx_searchtooltiptext");
			_filterApplyLabel = CreateLazyLocalizedString("adx_filter_applybuttonlabel");
		}

		public bool CreateEnabled { get { return CreateUrl != null; } }

		public string CreateLabel { get { return _createLabel.Value; } }

		public string CreateUrl { get { return _createUrl.Value; } }

		public bool DetailEnabled { get { return DetailUrl != null && !string.IsNullOrEmpty(DetailIdParameter); } }

		public string DetailIdParameter { get; private set; }

		public string DetailLabel { get { return _detailLabel.Value; } }

		public string DetailUrl { get { return _detailUrl.Value; } }

		public string EmptyListText { get { return _emptyListText.Value; } }

		public bool EnableEntityPermissions { get; private set; }

		public string EntityLogicalName { get; private set; }

		public string FilterAccountAttributeName { get; private set; }

		public string FilterApplyLabel { get { return _filterApplyLabel.Value; } }

		public string FilterDefinition { get; private set; }

		public bool FilterEnabled { get; private set; }

		public bool IsFilterVertical { get; private set; }

		public string FilterPortalUserAttributeName { get; private set; }

		public string FilterWebsiteAttributeName { get; private set; }

		public string GetDataUrl { get { return _getDataUrl.Value; } }

		public GridMetadataDrop GridMetadata { get { return _gridMetadata.Value; } }

		public int LanguageCode { get { return _languageCode.Value; } }

		public string Layouts { get { return _layouts.Value; } }

		public string ModalFormTemplateUrl { get { return _modalFormTemplateUrl.Value; } }

		public int? PageSize { get; private set; }

		public string PrimaryKeyName { get; private set; }

		public bool SearchEnabled { get; private set; }

		public string SearchPlaceholder { get { return _searchPlaceholder.Value; } }

		public string SearchTooltip { get { return _searchTooltip.Value; } }

		public IEnumerable<EntityListViewDrop> Views { get { return _views.Value; } }

		public Guid? DefaultViewId { get { return Views.Any() ? new Guid?(Views.First().ViewId) : null; } }

		internal GridMetadata GridMetadataConfiguration { get; private set; }

		internal ViewMetadata ViewMetadataConfiguration { get; private set; }

		private Lazy<string> CreateLazyLocalizedString(string attributeLogicalName)
		{
			return new Lazy<string>(() =>
			{
				var data = Entity.GetAttributeValue<string>(attributeLogicalName);

				var localized = Localization.GetLocalizedString(data, LanguageCode);

				return string.IsNullOrEmpty(localized) ? null : localized;

			}, LazyThreadSafetyMode.None);
		}

		private string GetLayouts(Entity entityList)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Getting Entity List For: {0}, entityId: {1}", entityList.LogicalName, entityList.Id));

			var viewConfigurations =
				GetEntityViews()
					.Select(
						view =>
							new ViewConfiguration(PortalViewContext.CreatePortalContext(),
								PortalViewContext.CreateServiceContext(), entityList, EntityLogicalName, PrimaryKeyName,
								view.Id, GridMetadataConfiguration, PortalViewContext.PortalName, LanguageCode,
								EnableEntityPermissions, "page", "filter", "query", "sort", "My", 20, "mf", null, null, null, null, null, view.DisplayName))
					.ToList();

			var layouts =
				viewConfigurations.Select(c =>
					{
						try
						{
							return new ViewLayout(c, null, PortalViewContext.PortalName, LanguageCode, false,
								(c.ItemActionLinks != null && c.ItemActionLinks.Any()));
						}
						catch (SavedQueryNotFoundException ex)
						{
							ADXTrace.Instance.TraceWarning(TraceCategory.Application, ex.Message);
							return null;
						}
					}).Where(l => l != null);


			return JsonConvert.SerializeObject(layouts);
		}

		private EntityView[] GetEntityViews()
		{
			if (ViewMetadataConfiguration == null)
			{
				return (Entity.GetAttributeValue<string>("adx_view") ?? string.Empty).Split(',').Select(e =>
				{
					Guid parsed;
					return Guid.TryParse(e.Trim(), out parsed) ? new Guid?(parsed) : null;
				}).Where(e => e.HasValue).Select(e =>
				{
					try
					{
						return new EntityView(PortalViewContext.CreateServiceContext(), e.Value, LanguageCode);
					}
					catch
					{
						return null;
					}
				}).ToArray();
			}
			
			return ViewMetadataConfiguration.Views.Select(
					e =>
						new EntityView(PortalViewContext.CreateServiceContext(), e.ViewId, LanguageCode,
							e.DisplayName != null ? Localization.GetLocalizedString(e.DisplayName, LanguageCode) : null)).ToArray();
		}

		private EntityListViewDrop[] GetViews()
		{
			if (ViewMetadataConfiguration == null)
			{
				return (Entity.GetAttributeValue<string>("adx_view") ?? string.Empty).Split(',').Select(e =>
				{
					Guid parsed;
					return Guid.TryParse(e.Trim(), out parsed) ? new Guid?(parsed) : null;
				}).Where(e => e.HasValue).Select(e =>
				{
					try
					{
						return new EntityView(PortalViewContext.CreateServiceContext(), e.Value, LanguageCode);
					}
					catch
					{
						return null;
					}
				}).Where(e => e != null).Select(e => new EntityListViewDrop(this, e)).ToArray();
			}

			return
				ViewMetadataConfiguration.Views.Select(e =>
				{
					try
					{
						return new EntityView(PortalViewContext.CreateServiceContext(), e.ViewId, LanguageCode,
							e.DisplayName != null ? Localization.GetLocalizedString(e.DisplayName, LanguageCode) : null);
					}
					catch
					{
						return null;
					}
				}).Where(e => e != null).Select(e => new EntityListViewDrop(this, e)).ToArray();
		}
	}
}
