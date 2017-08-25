/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet.Cms
{
	public class CrmWebsiteSetting
	{
		public virtual Entity Entity { get; private set; }
		public virtual string Name { get { return Entity.GetAttributeValue<string>("adx_name"); } }
		public virtual string Value { get { return Entity.GetAttributeValue<string>("adx_value"); } }

		public CrmWebsiteSetting(Entity entity)
		{
			Entity = entity;
		}
	}

	public class CrmWebsiteBinding
	{
		public virtual Entity Entity { get; private set; }
		public virtual string Name { get { return Entity.GetAttributeValue<string>("adx_name"); } }
		public virtual string SiteName { get { return Entity.GetAttributeValue<string>("adx_sitename"); } }
		public virtual string VirtualPath { get { return Entity.GetAttributeValue<string>("adx_virtualpath"); } }
		public virtual DateTime? ReleaseDate { get { return Entity.GetAttributeValue<DateTime?>("adx_releasedate"); } }
		public virtual DateTime? ExpirationDate { get { return Entity.GetAttributeValue<DateTime?>("adx_expirationdate"); } }

		public CrmWebsiteBinding(Entity entity)
		{
			Entity = entity;
		}
	}

	public class CrmWebsiteSettingCollection : List<CrmWebsiteSetting>
	{
		public CrmWebsiteSettingCollection(IEnumerable<CrmWebsiteSetting> collection)
			: base(collection)
		{
		}

		public T Get<T>(string name)
		{
			var setting = this.FirstOrDefault(s => s.Name.ToLower() == name.ToLower());

			if (setting == null) return default(T);

			var value = setting.Value;

			return GetValue<T>(value);
		}

		public T? GetEnum<T>(string name) where T : struct
		{
			var setting = this.FirstOrDefault(s => s.Name.ToLower() == name.ToLower());

			if (setting == null) return null;

			var value = setting.Value;

			T result;

			if (Enum.TryParse(value, out result))
			{
				return result;
			}

			return null;
		}

		protected virtual T GetValue<T>(string value)
		{
			var type = typeof(T);

			if (type.IsA(typeof(string)))
			{
				return (T)(object)value;
			}

			if (type.IsA(typeof(bool)) || type.IsA(typeof(bool?)))
			{
				bool result;

				if (bool.TryParse(value, out result))
				{
					return (T)(object)result;
				}
			}

			if (type.IsA(typeof(int)) || type.IsA(typeof(int?)))
			{
				int result;

				if (int.TryParse(value, out result))
				{
					return (T)(object)result;
				}
			}

			if (type.IsA(typeof(TimeSpan)) || type.IsA(typeof(TimeSpan?)))
			{
				TimeSpan result;

				if (TimeSpan.TryParse(value, out result))
				{
					return (T)(object)result;
				}
			}

			if (type.IsA(typeof(PathString)) || type.IsA(typeof(PathString?)))
			{
				return (T)(object)new PathString(value);
			}

			return default(T);
		}
	}

	public class CrmWebsite : CrmWebsite<Guid>, IDisposable
	{
		public CrmWebsite()
		{
		}

		public CrmWebsite(Entity entity)
			: base(entity)
		{
		}

		void IDisposable.Dispose() { }
	}

	public class CrmWebsite<TKey> : CrmModel<TKey>
	{
		private readonly Lazy<CrmWebsiteSettingCollection> _settings;

		public virtual CrmWebsiteSettingCollection Settings
		{
			get { return _settings.Value; }
		}

		private Lazy<IEnumerable<CrmWebsiteBinding>> _bindings;

		public virtual IEnumerable<CrmWebsiteBinding> Bindings
		{
			get { return _bindings.Value; }
		}

		/// <summary>
		/// The lcid of the language for the website
		/// </summary>
		public virtual int Language
		{
			get { return Entity.GetAttributeValue<int>("adx_website_language"); }
		}

		public virtual EntityReference DefaultLanguage
		{
			get { return Entity.GetAttributeValue<EntityReference>("adx_defaultlanguage"); }
		}

		public virtual EntityReference ParentWebsiteId
		{
			get { return Entity.GetAttributeValue<EntityReference>("adx_parentwebsiteid"); }
			set { Entity.SetAttributeValue("adx_parentwebsiteid", value); }
		}

		public virtual string PrimaryDomainName
		{
			get { return Entity.GetAttributeValue<string>("adx_primarydomainname"); }
			set { Entity.SetAttributeValue("adx_primarydomainname", value); }
		}

		public CrmWebsite()
			: this(null)
		{
		}

		public CrmWebsite(Entity entity)
			: base("adx_website", "adx_name", entity)
		{
			_settings = new Lazy<CrmWebsiteSettingCollection>(GetWebsiteSettings);
			_bindings = new Lazy<IEnumerable<CrmWebsiteBinding>>(GetWebsiteBindings);
		}

		protected virtual CrmWebsiteSettingCollection GetWebsiteSettings()
		{
			return new CrmWebsiteSettingCollection(GetRelatedEntities(WebsiteConstants.WebsiteSiteSettingRelationship).Select(ToSetting));
		}

		protected virtual CrmWebsiteSetting ToSetting(Entity entity)
		{
			return new CrmWebsiteSetting(entity);
		}

		protected virtual IEnumerable<CrmWebsiteBinding> GetWebsiteBindings()
		{
			return GetRelatedEntities(WebsiteConstants.WebsiteBindingRelationship).Select(ToBinding).ToList();
		}

		protected virtual CrmWebsiteBinding ToBinding(Entity entity)
		{
			return new CrmWebsiteBinding(entity);
		}

		protected virtual void RefreshWebsiteBindings()
		{
			_bindings = new Lazy<IEnumerable<CrmWebsiteBinding>>(GetWebsiteBindings);
		}

		public CrmWebsiteBinding AddWebsiteBinding(PortalHostingEnvironment environment)
		{
			var websiteBinding = new Entity("adx_websitebinding");
			websiteBinding.SetAttributeValue<string>("adx_name", "Binding: {0}".FormatWith(environment.SiteName));
			websiteBinding.SetAttributeValue<EntityReference>("adx_websiteid", this.Entity.ToEntityReference());
			websiteBinding.SetAttributeValue<string>("adx_sitename", environment.SiteName);

			if (this.Entity.RelatedEntities.ContainsKey(WebsiteConstants.WebsiteBindingRelationship))
			{
				this.Entity.RelatedEntities[WebsiteConstants.WebsiteBindingRelationship].Entities.Add(websiteBinding);
			}
			else
			{
				this.Entity.RelatedEntities.Add(WebsiteConstants.WebsiteBindingRelationship, new EntityCollection(new[] { websiteBinding }));
			}

			// reset binding collection
			this.RefreshWebsiteBindings();

			return this.ToBinding(websiteBinding);
		}

		public bool RemoveWebsiteBinding(CrmWebsiteBinding websiteBinding)
		{
			if (this.Entity.RelatedEntities.ContainsKey(WebsiteConstants.WebsiteBindingRelationship))
			{
				if (this.Entity.RelatedEntities[WebsiteConstants.WebsiteBindingRelationship].Entities.Remove(websiteBinding.Entity))
				{
					// reset binding collection
					this.RefreshWebsiteBindings();
				}
			}

			return false;
		}
	}
}
