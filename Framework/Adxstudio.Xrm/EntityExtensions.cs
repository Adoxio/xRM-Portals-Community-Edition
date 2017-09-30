/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Tagging;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm
{
	/// <summary>
	/// Helper methods on the <see cref="Entity"/> class.
	/// </summary>
	public static class EntityExtensions
	{
		/// <summary>
		/// Retrieves the value of an attribute that may be aliased as a result of a join operation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static T GetAttributeAliasedValue<T>(this Entity entity, string attributeLogicalName, string alias = null)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (attributeLogicalName == null) throw new ArgumentNullException("attributeLogicalName");

			var prefix = !string.IsNullOrWhiteSpace(alias) ? alias + "." : string.Empty;
			var raw = entity.GetAttributeValue(prefix + attributeLogicalName);
			var aliasdValue = raw as AliasedValue;
			var intermediate = aliasdValue != null ? aliasdValue.Value : raw;
			var value = GetPrimitiveValue<T>(intermediate);
			return value != null ? (T)value : default(T);
		}

		/// <summary>
		/// Retrieves the <see cref="Enum"/> value for an option set attribute of the entity.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static TEnum? GetAttributeEnumValue<TEnum>(this Entity entity, string attributeLogicalName, string alias = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var option = GetAttributeAliasedValue<OptionSetValue>(entity, attributeLogicalName, alias);
			return option != null ? option.Value.ToEnum<TEnum>() : (TEnum?)null;
		}

		/// <summary>
		/// Retrieves the value of the identifier attribute that may be aliased as a result of a join operation.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="alias"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetEntityIdentifier<T>(this Entity entity, string attributeLogicalName, string alias = null) where T : EntityNode
		{
			var er = GetAttributeAliasedValue<EntityReference>(entity, attributeLogicalName, alias);
			var id = er != null ? Activator.CreateInstance(typeof(T), er) as T : null;
			return id;
		}

		/// <summary>
		/// Retrieves the value of an interstect's identifier attribute that may be aliased as a result of a join operation.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="alias"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetIntersectEntityIdentifier<T>(this Entity entity, string entityLogicalName, string attributeLogicalName, string alias = null) where T : EntityNode
		{
			var guid = GetAttributeAliasedValue<Guid?>(entity, attributeLogicalName, alias);
			var er = guid != null ? new EntityReference(entityLogicalName, guid.Value) : null;
			var id = er != null ? Activator.CreateInstance(typeof(T), er) as T : null;
			return id;
		}

		private static object GetPrimitiveValue<T>(object value)
		{
			if (value is T) return value;
			if (value == null) return default(T);

			if (value is OptionSetValue && typeof(T).GetUnderlyingType() == typeof(int))
			{
				return (value as OptionSetValue).Value;
			}

			if (value is EntityReference && typeof(T).GetUnderlyingType() == typeof(Guid))
			{
				return (value as EntityReference).Id;
			}

			if (value is Money && typeof(T).GetUnderlyingType() == typeof(decimal))
			{
				return (value as Money).Value;
			}

			if (value is CrmEntityReference && typeof(T).GetUnderlyingType() == typeof(EntityReference))
			{
				var reference = value as CrmEntityReference;
				return new EntityReference(reference.LogicalName, reference.Id) { Name = reference.Name };
			}

			return value;
		}

		/// <summary>
		/// Retrieves <see cref="PageTagInfo"/> | <see cref="ForumThreadTagInfo"/> | <see cref="EventTagInfo"/>
		/// </summary>
		/// <param name="entity">Entity</param>
		/// <returns><see cref="ITagInfo"/></returns>
		public static ITagInfo GetTagInfo(this Entity entity)
		{
			if (entity == null) return null;

			var entityName = entity.LogicalName;

			if (entityName == "adx_pagetag") return new PageTagInfo(entity);
			if (entityName == "adx_communityforumthreadtag") return new ForumThreadTagInfo(entity);
			if (entityName == "adx_eventtag") return new EventTagInfo(entity);

			return null;
		}

		internal static void AssertEntityName(this Entity entity, params string[] expectedEntityName)
		{
			// accept null values

			if (entity == null) return;

			if (!HasLogicalName(entity, expectedEntityName))
			{
				throw new ArgumentException(
					ResourceManager.GetString("Extension_Method_Expected_IsDifferent_Exception").FormatWith(
						string.Join(" or ", expectedEntityName),
						entity.LogicalName));
			}
		}

		private static bool HasLogicalName(this Entity entity, params string[] expectedEntityName)
		{
			return entity != null && expectedEntityName.Contains(entity.LogicalName);
		}

		/// <summary>
		/// Given a default value, this extension will return the value of the named attribute, or the default value if null.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="attributeName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string GetAttributeValueOrDefault(this Entity entity, string attributeName, string defaultValue)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			object attributeValue;

			if (entity.Attributes.TryGetValue(attributeName, out attributeValue))
			{
				if (attributeValue != null)
				{
					var value = attributeValue.ToString();

					if (!string.IsNullOrEmpty(value))
					{
						return value;
					}
				}
			}

			return defaultValue;
		}

		/// <summary>
		/// Get the label of an entity's option set value.
		/// </summary>
		/// <param name="entity">Entity</param>
		/// <param name="entityMetadata">Entity metadata</param>
		/// <param name="attributeLogicalName">Logical name of the option set attribute</param>
		/// <param name="languageCode">Optional language code used to return the localized label</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string GetEnumLabel(this Entity entity, EntityMetadata entityMetadata, string attributeLogicalName, int? languageCode)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
			if (string.IsNullOrWhiteSpace(attributeLogicalName)) throw new ArgumentNullException("attributeLogicalName");

			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName) as EnumAttributeMetadata;

			if (attributeMetadata == null)
			{
				return null;
			}

			var value = entity.GetAttributeValue<OptionSetValue>(attributeLogicalName);

			if (value == null)
			{
				return null;
			}

			var option = attributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == value.Value);

			if (option == null)
			{
				return null;
			}

			var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == (languageCode ?? 0));

			var label = localizedLabel == null ? option.Label.GetLocalizedLabelString() : localizedLabel.Label;

			return label;
		}

		/// <summary>
		/// Modifies the value of an attribute and truncates the string to the max length specified for the attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="serviceContext"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		/// <param name="metadataCache"></param>
		public static void SetAttributeStringTruncatedToMaxLength(this Entity entity, OrganizationServiceContext serviceContext, string attributeLogicalName, string value, IDictionary<string, EntityMetadata> metadataCache)
		{
			var entityMetadata = serviceContext.GetEntityMetadata(entity.LogicalName, metadataCache);

			entity.SetAttributeStringTruncatedToMaxLength(entityMetadata, attributeLogicalName, value);
		}

		/// <summary>
		/// Modifies the value of an attribute and truncates the string to the max length specified for the attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="serviceContext"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeStringTruncatedToMaxLength(this Entity entity, OrganizationServiceContext serviceContext, string attributeLogicalName, string value)
		{
			var entityMetadata = serviceContext.GetEntityMetadata(entity.LogicalName);

			entity.SetAttributeStringTruncatedToMaxLength(entityMetadata, attributeLogicalName, value);
		}

		/// <summary>
		/// Modifies the value of an attribute and truncates the string to the max length specified for the attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeStringTruncatedToMaxLength(this Entity entity, EntityMetadata entityMetadata, string attributeLogicalName, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (entityMetadata == null)
				{
					throw new ApplicationException("Unable to retrieve the entity metadata for {0}.".FormatWith(entity.LogicalName));
				}

				if (!entityMetadata.Attributes.Select(a => a.LogicalName).Contains(attributeLogicalName))
				{
					throw new ApplicationException("Attribute {0} could not be found in entity metadata for {1}.".FormatWith(attributeLogicalName, entity.LogicalName));
				}

				var attribute = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

				if (attribute == null || attribute.AttributeType != AttributeTypeCode.String)
				{
					throw new ApplicationException("Attribute {0} is not of type string.".FormatWith(attributeLogicalName));
				}

				var stringAttributeMetadata = attribute as StringAttributeMetadata;

				if (stringAttributeMetadata != null)
				{
					var maxLength = stringAttributeMetadata.MaxLength ?? 0;

					if (maxLength > 0 && value.Length > maxLength)
					{
                        ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"String length ({0}) is greater than attribute ""{1}"" max length ({2}). String has been truncated.", value.Length, attributeLogicalName, maxLength));
						value = value.Truncate(maxLength);
					}
				}
			}

			entity.SetAttributeValue<string>(attributeLogicalName, value);
		}

		/// <summary>
		/// Retrieve the url to emit that will download an attached file from the website.
		/// </summary>
		public static string GetFileAttachmentUrl(this Entity entity, Entity website)
		{
			return website == null ? GetFileAttachmentUrl(entity) : GetFileAttachmentUrl(entity, website.Id);
		}

		/// <summary>
		/// Retrieve the url to emit that will download an attached file from the website.
		/// </summary>
		public static string GetFileAttachmentUrl(this Entity entity, EntityReference website)
		{
			return website == null ? GetFileAttachmentUrl(entity) : GetFileAttachmentUrl(entity, website.Id);
		}

		/// <summary>
		/// Retrieve the url to emit that will download an attached file from the website.
		/// </summary>
		public static string GetFileAttachmentUrl(this Entity entity, Guid? websiteId = null)
		{
			var path = GetFileAttachmentPath(entity, websiteId);

			return path == null ? null : path.AbsolutePath;
		}

		/// <summary>
		/// Retrieve the path to emit that will download an attached file from the website.
		/// </summary>
		public static ApplicationPath GetFileAttachmentPath(this Entity entity, Entity website)
		{
			return website == null ? GetFileAttachmentPath(entity) : GetFileAttachmentPath(entity, website.Id);
		}

		/// <summary>
		/// Retrieve the path to emit that will download an attached file from the website.
		/// </summary>
		public static ApplicationPath GetFileAttachmentPath(this Entity entity, EntityReference website)
		{
			return website == null ? GetFileAttachmentPath(entity) : GetFileAttachmentPath(entity, website.Id);
		}

		/// <summary>
		/// Retrieve the path to emit that will download an attached file from the website.
		/// </summary>
		public static ApplicationPath GetFileAttachmentPath(this Entity entity, Guid? websiteId = null)
		{
			if (entity == null) return null;

			var http = HttpContext.Current;

			if (http == null) return null;

			var requestContext = new RequestContext(new HttpContextWrapper(http), new RouteData());

			VirtualPathData virtualPath;

			if (websiteId == null)
			{
				virtualPath = RouteTable.Routes.GetVirtualPath(requestContext, typeof(EntityRouteHandler).FullName,
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", entity.LogicalName },
						{ "id", entity.Id }
					});
			}
			else
			{
				virtualPath = RouteTable.Routes.GetVirtualPath(requestContext, typeof(EntityRouteHandler).FullName + "PortalScoped", new RouteValueDictionary
				{
					{ "prefix", "_entity" },
					{ "logicalName", entity.LogicalName },
					{ "id", entity.Id },
					{ "__portalScopeId__", websiteId }
				});
			}

			return virtualPath == null
				? null
				: ApplicationPath.FromAbsolutePath(VirtualPathUtility.ToAbsolute(virtualPath.VirtualPath));
		}

		/// <summary>
		/// Returns an <see cref="EntityReference"/> to a language container for the given entity. Only web pages currently have a language container (root web page).
		/// </summary>
		/// <param name="entity">Entity to analyze.</param>
		/// <returns>An <see cref="EntityReference"/> to the language container for web pages (root web page). Otherwise an <see cref="EntityReference"/> to the given entity.</returns>
		public static EntityReference ToLanguageContainerEntityReference(this Entity entity)
		{
			if (entity.LogicalName == "adx_webpage")
			{
				var rootWebPage = entity.GetAttributeValue<EntityReference>("adx_rootwebpageid");
				if (rootWebPage != null)
				{
					return rootWebPage;
				}
			}

			return entity.ToEntityReference();
		}
	}
}
