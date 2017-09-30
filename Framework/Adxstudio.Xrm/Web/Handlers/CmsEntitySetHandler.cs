/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Web.Routing;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntitySetHandler : IHttpHandler
	{
		protected const string JsonMimeType = "application/json";

		private const string ExtensionsKey = "__extensions";
		private const string ExtensionsPrefix = ExtensionsKey + ".";

		private static readonly Encoding _contentEncoding = Encoding.UTF8;

		public CmsEntitySetHandler() : this(null, null, null) { }

		public CmsEntitySetHandler(string portalName, Guid? portalScopeId, string entityLogicalName)
		{
			PortalName = portalName;
			PortalScopeId = portalScopeId;
			EntityLogicalName = entityLogicalName;
		}

		public virtual bool IsReusable
		{
			get { return false; }
		}

		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		public virtual Guid? PortalScopeId { get; private set; }

		protected virtual Encoding ContentEncoding
		{
			get { return _contentEncoding; }
		}

		protected virtual string ContentType
		{
			get { return JsonMimeType; }
		}

		/// <summary>
		/// The CRM logical name of the entity whose attribute is being requested.
		/// </summary>
		protected virtual string EntityLogicalName { get; private set; }

		public virtual void ProcessRequest(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			try
			{
				context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

				Guid parsedPortalScopeId;
				var portalScopeId = PortalScopeId ?? (Guid.TryParse(context.Request.Params["__portalScopeId__"], out parsedPortalScopeId) ? new Guid?(parsedPortalScopeId) : null);

				if (portalScopeId == null)
				{
					throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine portal scope ID from request.");
				}

				var entityLogicalName = string.IsNullOrWhiteSpace(EntityLogicalName) ? context.Request.Params["entityLogicalName"] : EntityLogicalName;

				if (string.IsNullOrWhiteSpace(entityLogicalName))
				{
					throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine entity logical name from request.");
				}

				var serviceProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICmsEntityServiceProvider>();

				if (serviceProvider == null)
				{
					throw new CmsEntityServiceException(HttpStatusCode.InternalServerError, "Unable to get dependency {0}. Please check the configured portal dependency provider.".FormatWith(typeof(ICmsEntityServiceProvider).FullName));
				}

				var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName, context.Request.RequestContext);
				var serviceContext = CreateServiceContext();
				
				ProcessRequest(context, serviceProvider, portalScopeId.Value, portal, serviceContext, entityLogicalName);
			}
			catch (CmsEntityServiceException e)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());

                WriteErrorResponse(context.Response, e.StatusCode, e);
			}
			catch (Exception e)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());

                WriteErrorResponse(context.Response, HttpStatusCode.InternalServerError, e);
			}
		}

		protected OrganizationServiceContext CreateServiceContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		protected virtual void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			var entityMetadata = new CmsEntityMetadata(serviceContext, entityLogicalName);

			var entities = serviceProvider.ExecuteEntitySetQuery(context, portal, serviceContext, entityLogicalName, entityMetadata, context.Request.Params["filter"]);
			
			WriteResponse(context.Response, new JObject
			{
				{ "d", new JArray(entities.Select(e => GetEntityReferenceJson(e, entityMetadata, portalScopeId))) }
			});
		}

		protected virtual bool IsRequestMethod(HttpRequest request, string httpMethod)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			return string.Equals(request.HttpMethod, httpMethod, StringComparison.InvariantCultureIgnoreCase);
		}

		protected JObject GetEntityJson(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			var json = new JObject
			{
				{
					"__metadata", new JObject
					{
						{ "uri", new JValue(VirtualPathUtility.ToAbsolute(CmsEntityRouteHandler.GetAppRelativePath(portalScopeId, entity.ToEntityReference()))) },
						{ "type", new JValue(entity.GetType().FullName) },
					}
				},
				{ "Id", new JValue(entity.Id.ToString()) },
				{ "LogicalName", new JValue(entity.LogicalName) },
			};

			foreach (var attributeLogicalName in entityMetadata.Attributes)
			{
				json[attributeLogicalName] = entity.Attributes.Contains(attributeLogicalName)
					? GetValueJson(entity.Attributes[attributeLogicalName])
					: null;
			}

			var extensions = new JObject();

			json[ExtensionsKey] = extensions;

			serviceProvider.ExtendEntityJson(context, portal, serviceContext, entity, entityMetadata, extensions);

			foreach (var relationship in entityMetadata.Relationships)
			{
				json[relationship.ToSchemaName(".")] = new JObject
				{
					{
						"__deferred", new JObject
						{
							{ "uri", new JValue(VirtualPathUtility.ToAbsolute(CmsEntityRelationshipRouteHandler.GetAppRelativePath(portalScopeId, entity.ToEntityReference(), relationship))) }
						}
					},
				};
			}

			return json;
		}

		protected JObject GetEntityReferenceJson(Entity entity, CmsEntityMetadata entityMetadata, Guid portalScopeId)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			var primaryNameAttribute = entityMetadata.PrimaryNameAttribute;
			var name = entity.Attributes.Contains(primaryNameAttribute) ? entity.GetAttributeValue<string>(primaryNameAttribute) : null;

			var json = new JObject
			{
				{
					"__metadata", new JObject
					{
						{ "uri", new JValue(VirtualPathUtility.ToAbsolute(CmsEntityRouteHandler.GetAppRelativePath(portalScopeId, entity.ToEntityReference()))) },
						{ "type", new JValue(entity.GetType().FullName) },
					}
					},
				{ "Id", new JValue(entity.Id.ToString()) },
				{ "LogicalName", new JValue(entity.LogicalName) },
				{ "Name", new JValue(name) },
				{ primaryNameAttribute, new JValue(name) },
				{ entityMetadata.PrimaryIdAttribute, new JValue(entity.Id.ToString()) }
			};

			if (entityMetadata.HasAttribute("adx_description"))
			{
				json["adx_description"] = new JValue(entity.GetAttributeValue<string>("adx_description"));
			}

			if (entityMetadata.HasAttribute("adx_isdefault"))
			{
				json["adx_isdefault"] = new JValue(entity.Attributes.Contains("adx_isdefault") && entity.GetAttributeValue<bool?>("adx_isdefault").GetValueOrDefault(false));
			}

			if (entityMetadata.HasAttribute("adx_isvisible"))
			{
				json["adx_isvisible"] = new JValue(entity.Attributes.Contains("adx_isvisible") && entity.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault(false));
			}

			return json;
		}

		protected JToken GetValueJson(object value)
		{
			var optionSetValue = value as OptionSetValue;

			if (optionSetValue != null)
			{
				return new JValue(optionSetValue.Value);
			}

			var crmEntityReference = value as CrmEntityReference;

			if (crmEntityReference != null)
			{
				return new JObject
				{
					{
						"__metadata", new JObject
						{
							{ "type", new JValue(crmEntityReference.GetType().FullName) }
						}
					},
					{ "Id", new JValue(crmEntityReference.Id) },
					{ "LogicalName", new JValue(crmEntityReference.LogicalName) },
					{ "Name", new JValue(crmEntityReference.Name) },
				};
			}

			var entityReference = value as EntityReference;

			if (entityReference != null)
			{
				return new JObject
				{
					{
						"__metadata", new JObject
						{
							{ "type", new JValue(entityReference.GetType().FullName) }
						}
					},
					{ "Id", new JValue(entityReference.Id) },
					{ "LogicalName", new JValue(entityReference.LogicalName) },
					{ "Name", new JValue(entityReference.Name) },
				};
			}

			var dateTime = value as DateTime?;

			if (dateTime != null)
			{
				return new JRaw(JsonConvert.SerializeObject(dateTime.Value.ToUniversalTime(), new JsonSerializerSettings
				{
					DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
				}));
			}

			var money = value as Money;

			if (money != null)
			{
				return new JValue(money.Value);
			}

			return new JValue(value);
		}

		protected virtual JObject UpdateEntityFromJsonRequestBody(HttpRequest request, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata)
		{
			if (!request.ContentType.StartsWith(JsonMimeType, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new CmsEntityServiceException(HttpStatusCode.UnsupportedMediaType, "Request Content-Type {0} is not supported by this resource.".FormatWith(request.ContentType));
			}

			using (var reader = new StreamReader(request.InputStream))
			{
				JObject json;
				
				try
				{
					json = JObject.Parse(reader.ReadToEnd());
				}
				catch (Exception e)
				{
					throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Failed to parse request body as {0}.".FormatWith(JsonMimeType), e);
				}
                
				var updateFactory = new EntityAttributeUpdateFactory((entityLogicalName, attributeLogicalName) =>
				{
					AttributeMetadata attributeMetadata;

					return (entityLogicalName == entityMetadata.LogicalName && entityMetadata.TryGetAttributeMetadata(attributeLogicalName, out attributeMetadata))
						? attributeMetadata
						: null;
				});

				foreach (var property in json.Properties().Where(p => !p.Name.StartsWith(ExtensionsPrefix)))
				{
					var update = updateFactory.Create(serviceContext, entity, property.Name);

					update.Apply(property.Value);
				}

				return new JObject(json.Properties().Where(p => p.Name.StartsWith(ExtensionsPrefix)).Select(p => new JProperty(p.Name.Substring(ExtensionsPrefix.Length), p.Value)));
			}
		}

		protected virtual void WriteErrorResponse(HttpResponse response, HttpStatusCode statusCode, Exception e)
		{
			if (e == null)
			{
				throw new ArgumentNullException("e");
			}
		    var guid = WebEventSource.Log.GenericErrorException(e);
			var json = new JObject
			{
				{
					"error", new JObject
					{
						{ "code", new JValue(string.Empty) },
						{
							"message", new JObject
							{
								{ "value", new JValue(string.Format(Resources.ResourceManager.GetString("Generic_Error_Message"), guid)) }
							}
						},
					}
				}
			};

			response.StatusCode = (int)statusCode;
			response.ContentEncoding = ContentEncoding;
			response.ContentType = ContentType;
			response.Write(json.ToString());
		}

		protected virtual void WriteNoContentResponse(HttpResponse response)
		{
			response.StatusCode = (int)HttpStatusCode.NoContent;
		}

		protected virtual void WriteResponse(HttpResponse response, JObject json, HttpStatusCode? statusCode = null)
		{
			if (statusCode.HasValue)
			{
				response.StatusCode = (int)(statusCode.Value);
			}

			response.ContentEncoding = ContentEncoding;
			response.ContentType = ContentType;
			response.Write(json.ToString());
		}

		protected virtual IWebsiteAccessPermissionProvider CreateWebsiteAccessPermissionProvider(IPortalContext portal)
		{
			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);

			return new WebsiteAccessPermissionProvider(portal.Website, contentMapProvider);
		}
	}
}
