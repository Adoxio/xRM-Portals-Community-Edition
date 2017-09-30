/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Web.Hosting;
using System.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Site.Areas.Setup.Models;
using Adxstudio.Xrm.Core.Telemetry;
using Adxstudio.Xrm;

namespace Site.Areas.Setup.Controllers
{
	[NoCache]
	public class SetupController : Controller
	{
		[Serializable]
		private class ModelErrorException : Exception
		{
			public string Key { get; private set; }

			public ModelErrorException(string key, string message, Exception innerException)
				: base(message, innerException)
			{
				Key = key;
			}
		}

		private static readonly string _orgSvcPath = "/XRMServices/2011/Organization.svc";

		[HttpGet]
		public ActionResult Index()
		{
			return View(new SetupViewModel());
		}

		[HttpPost]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Index(SetupViewModel setup)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Save(setup);

					if (ModelState.IsValid)
					{
						Adxstudio.Xrm.Web.Extensions.RestartWebApplication();

						return Redirect("~/");
					}
				}
			}
			catch (ModelErrorException mee)
			{
				ModelState.AddModelError(mee.Key, ToErrorMessage(mee));
			}
			catch (Exception e)
			{
				ModelState.AddModelError(string.Empty, ToErrorMessage(e));
			}

			return Index();
		}

		[HttpPost]
		[AjaxValidateAntiForgeryToken]
		public ActionResult OrganizationConfiguration(Uri url)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("url={0}", url));
			
			try
			{
				var serviceUri = GetOrganizationServiceUrl(url);
				var authenticationType = GetAuthenticationType(serviceUri);

				return Json(new { authenticationType = authenticationType.ToString() });
			}
			catch (ModelErrorException mee)
			{
				return ToJsonModelError(mee.Key, mee);
			}
			catch (Exception e)
			{
				return ToJsonModelError("OrganizationServiceUrl", e);
			}
		}

		[HttpGet]
		public ActionResult Provisioning()
		{
			if (!SetupConfig.ProvisioningInProgress())
			{
				Adxstudio.Xrm.Web.Extensions.RestartWebApplication();

				return Redirect("~/");
			}

			return View();
		}

		[HttpPost]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Websites(Uri url, string username, string password)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("url={0}", url));

			try
			{
				var serviceUri = GetOrganizationServiceUrl(url);
				var authenticationType = GetAuthenticationType(serviceUri);

				var connection = GetConnection(serviceUri, authenticationType, username, password);

				using (var service = new OrganizationService(connection))
				{
					var query = new QueryExpression("adx_website") { ColumnSet = new ColumnSet("adx_name") };

					query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
					query.Criteria.AddCondition("adx_parentwebsiteid", ConditionOperator.Null);

					var linkEntity = new LinkEntity("adx_website", "adx_websitebinding", "adx_websiteid", "adx_websiteid",
						JoinOperator.LeftOuter);
					linkEntity.Columns.AddColumn("adx_websitebindingid");
					linkEntity.EntityAlias = "binding";
					linkEntity.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
					linkEntity.LinkCriteria.AddCondition("adx_sitename", ConditionOperator.Equal, SetupManager.GetSiteName());

					var filter = linkEntity.LinkCriteria.AddFilter(LogicalOperator.Or);
					var path = HostingEnvironment.ApplicationVirtualPath ?? "/";
					if (!path.StartsWith("/")) path = "/" + path;
					filter.AddCondition("adx_virtualpath", ConditionOperator.Equal, path);
					filter.AddCondition("adx_virtualpath", ConditionOperator.Equal, path.Substring(1));
					if (path.Substring(1) == string.Empty)
					{
						filter.AddCondition("adx_virtualpath", ConditionOperator.Null);
					}

					query.LinkEntities.Add(linkEntity);

					var entities = service.RetrieveMultiple(query).Entities;
					var websites = entities
						.Select(w => new
						{
							Name = w.GetAttributeValue<string>("adx_name"),
							Id = w.GetAttributeValue<Guid>("adx_websiteid"),
							Binding = w.GetAttributeValue<AliasedValue>("binding.adx_websitebindingid") != null
						})
						.OrderBy(w => w.Name)
						.ToList();

					if (!websites.Any())
					{
						throw new ModelErrorException("Website", ResourceManager.GetString("No_Active_Website_Found_Exception"), null);
					}

					return Json(websites);
				}
			}
			catch (ModelErrorException mee)
			{
				return ToJsonModelError(mee.Key, mee);
			}
			catch (Exception e)
			{
				return ToJsonModelError("Website", e);
			}
		}

		private static CrmConnection GetConnection(Uri serviceUri, AuthenticationProviderType authenticationType, string username, string password)
		{
			if (authenticationType == AuthenticationProviderType.ActiveDirectory)
			{
				var parts = username.Split(new[] { '\\' });

				if (parts.Length != 2)
				{
					throw new ModelErrorException("Username", ResourceManager.GetString("ModelErrorException_For_User_Name"), null);
				}

				return GetConnection(serviceUri, parts[0], parts[1], password);
			}

			return GetConnection(serviceUri, null, username, password);
		}

		private static CrmConnection GetConnection(Uri serviceUri, string domain, string username, string password)
		{
			var credentals = new ClientCredentials();

			if (domain != null)
			{
				credentals.Windows.ClientCredential.Domain = domain;
				credentals.Windows.ClientCredential.UserName = username;
				credentals.Windows.ClientCredential.Password = password;
			}
			else
			{
				credentals.UserName.UserName = username;
				credentals.UserName.Password = password;
			}

			var connection = new CrmConnection
			{
				ServiceUri = serviceUri,
				ClientCredentials = credentals,
				ServiceConfigurationInstanceMode = ServiceConfigurationInstanceMode.PerInstance,
				UserTokenExpiryWindow = TimeSpan.Zero,
			};

			try
			{
				using (var service = new OrganizationService(connection))
				{
					service.Execute(new WhoAmIRequest());
				}
			}
			catch (Exception e)
			{
				throw new ModelErrorException("Password", ResourceManager.GetString("Invalid_Username_Or_Password_Exception"), e);
			}

			return connection;
		}

		private static AuthenticationProviderType GetAuthenticationType(Uri serviceUri)
		{
			try
			{
				var config = ServiceConfigurationFactory.CreateConfiguration<IOrganizationService>(serviceUri);
				return config.AuthenticationType;
			}
			catch (Exception e)
			{
				throw new ModelErrorException("OrganizationServiceUrl", ResourceManager.GetString("Organization_Service_URL_IsInvalid_Exception"), e);
			}
		}

		private static Uri GetOrganizationServiceUrl(Uri url)
		{
			return url.AbsolutePath.EndsWith(_orgSvcPath, StringComparison.OrdinalIgnoreCase)
				? url
				: new Uri(url, url.AbsolutePath.TrimEnd('/') + _orgSvcPath);
		}

		private static void Save(SetupViewModel setup)
		{
			var serviceUri = GetOrganizationServiceUrl(setup.OrganizationServiceUrl);
			var authenticationType = GetAuthenticationType(serviceUri);

			var connection = GetConnection(serviceUri, authenticationType, setup.Username, setup.Password);

			if (authenticationType == AuthenticationProviderType.ActiveDirectory)
			{
				SetupConfig.SetupManager.Save(
					connection.ServiceUri,
					authenticationType,
					connection.ClientCredentials.Windows.ClientCredential.Domain,
					connection.ClientCredentials.Windows.ClientCredential.UserName,
					connection.ClientCredentials.Windows.ClientCredential.Password,
					setup.Website);
			}
			else
			{
				SetupConfig.SetupManager.Save(
					connection.ServiceUri,
					authenticationType,
					null,
					connection.ClientCredentials.UserName.UserName,
					connection.ClientCredentials.UserName.Password,
					setup.Website);
			}
		}
		
		private static IEnumerable<Exception> ToExceptionPath(Exception exception)
		{
			yield return exception;

			if (exception.InnerException != null)
			{
				foreach (var inner in ToExceptionPath(exception.InnerException))
				{
					yield return inner;
				}
			}
		}

		private static string ToErrorMessage(Exception exception)
		{
			return string.Join(" ", ToExceptionPath(exception).Select(e => e.Message).Take(5).ToArray());
		}

		private JsonResult ToJsonModelError(string key, Exception exception)
		{
			return ToJsonModelError(key, ToErrorMessage(exception));
		}

		private JsonResult ToJsonModelError(string key, string errorMessage)
		{
			Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return Json(new { key, errorMessage });
		}
	}
}
