/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services.Query;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Condition = Adxstudio.Xrm.Services.Query.Condition;
using Filter = Adxstudio.Xrm.Services.Query.Filter;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityFileSystem : IComposableFileSystem
	{
		public EntityFileSystem(IPortalViewContext portalViewContext, string entityLogicalName, string nameAttributeLogicalName, string sourceAttributeLogicalName)
		{
			if (portalViewContext == null) throw new ArgumentNullException("portalViewContext");
			if (entityLogicalName == null) throw new ArgumentNullException("entityLogicalName");
			if (nameAttributeLogicalName == null) throw new ArgumentNullException("nameAttributeLogicalName");
			if (sourceAttributeLogicalName == null) throw new ArgumentNullException("sourceAttributeLogicalName");

			PortalViewContext = portalViewContext;
			EntityLogicalName = entityLogicalName;
			NameAttributeLogicalName = nameAttributeLogicalName;
			SourceAttributeLogicalName = sourceAttributeLogicalName;
		}

		public string EntityLogicalName { get; private set; }

		public string NameAttributeLogicalName { get; private set; }

		public IPortalViewContext PortalViewContext { get; private set; }

		public string SourceAttributeLogicalName { get; private set; }

		public IEnumerable<TemplateFileInfo> GetTemplateFiles()
		{
			var website = PortalViewContext.Website.EntityReference;

			var fetch = new Fetch
			{
				Entity = new FetchEntity(EntityLogicalName, new[] { NameAttributeLogicalName })
				{
					Filters = new[]
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new[]
							{
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_websiteid", ConditionOperator.Equal, website.Id)
							}
						}
					},
					Orders = new[]
					{
						new Order(NameAttributeLogicalName, OrderType.Ascending), 
					}
				}
			};

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

				return response.EntityCollection.Entities
					.Select(e => new TemplateFileInfo(e.GetAttributeValue<string>(NameAttributeLogicalName)));
			}
		}

		public string ReadTemplateFile(Context context, string templateName)
		{
			string template;

			if (TryReadTemplateFile(context, templateName, out template))
			{
				return template;
			}

			throw new FileSystemException("Template {0} not found.", context[templateName] as string);
		}

		public bool TryReadTemplateFile(Context context, string templateName, out string template)
		{
			template = null;

			var entityName = (string)context[templateName];

			HtmlHelper htmlHelper;

			if (!TryGetHtmlHelper(context, out htmlHelper))
			{
				return false;
			}

			var cacheKey = "liquid:template:{0}".FormatWith(entityName);

			object cached;

			if (htmlHelper.ViewContext.TempData.TryGetValue(cacheKey, out cached) && cached is string)
			{
				template = (string)cached;

				return true;
			}

			if (TryReadTemplateFile(entityName, out template))
			{
				htmlHelper.ViewContext.TempData[cacheKey] = template;

				return true;
			}

			return false;
		}

		public bool TryReadTemplateFile(string templateName, out string template)
		{
			var website = PortalViewContext.Website.EntityReference;

			var fetch = new Fetch
			{
				PageSize = 1,
				Entity = new FetchEntity(EntityLogicalName, new[] { SourceAttributeLogicalName })
				{
					Filters = new[]
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new[]
							{
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
								new Condition(NameAttributeLogicalName, ConditionOperator.Equal, templateName)
							}
						}
					}
				}
			};

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());
				var entity = response.EntityCollection.Entities.FirstOrDefault();

				if (entity == null)
				{
					template = null;

					return false;
				}

				template = entity.GetAttributeValue<string>(SourceAttributeLogicalName) ?? string.Empty;

				return true;
			}
		}

		private bool TryGetHtmlHelper(Context context, out HtmlHelper html)
		{
			html = null;

			object register;

			if (!context.Registers.TryGetValue("htmlHelper", out register))
			{
				return false;
			}

			html = register as HtmlHelper;

			return html != null;
		}
	}
}
