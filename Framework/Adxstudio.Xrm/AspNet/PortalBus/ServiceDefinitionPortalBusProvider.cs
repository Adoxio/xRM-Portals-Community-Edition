/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using Adxstudio.Xrm.WindowsAzure.ServiceRuntime.Configuration;
using Microsoft.Owin;
using Microsoft.Xrm.Client;
using Owin;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// Settings related to the <see cref="ServiceDefinitionPortalBusProvider{TMessage}"/>.
	/// </summary>
	public class ServiceDefinitionPortalBusOptions<TMessage> : PortalBusOptions<TMessage>
	{
		public string InternalEndpointName { get; set; }
		public ServiceDefinition ServiceDefinition { get; set; }

		public ServiceDefinitionPortalBusOptions()
		{
			InternalEndpointName = "XrmEndpoint";
		}
	}

	/// <summary>
	/// A portal bus that uses web application endpoints for sending messages to remote instances.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public class ServiceDefinitionPortalBusProvider<TMessage> : PortalBusProvider<TMessage>
	{
		protected ServiceDefinitionPortalBusOptions<TMessage> Options { get; private set; }
		protected ServiceDefinition FullServiceDefinition { get; private set; }

		public ServiceDefinitionPortalBusProvider(IAppBuilder app, ServiceDefinitionPortalBusOptions<TMessage> options)
			: base(app)
		{
			FullServiceDefinition = InternalMerge(options.ServiceDefinition ?? new ServiceDefinition());
			Options = options;
		}

		protected override async Task SendRemoteAsync(IOwinContext context, TMessage message)
		{
			IPAddress localAddress = null;

			if (context != null)
			{
				IPAddress.TryParse(context.Environment["server.LocalIpAddress"] as string ?? string.Empty, out localAddress);
			}

			var roles = GetRoles(FullServiceDefinition, false);
			var sites = FullServiceDefinition.CurrentRole.Sites;

			// send the message

			await SendRequest(roles, sites, localAddress, Options.InternalEndpointName, message).WithCurrentCulture();
		}

		private ServiceDefinition InternalMerge(ServiceDefinition serviceDefinition)
		{
			return Merge(serviceDefinition);
		}

		private static IEnumerable<Role> GetRoles(ServiceDefinition serviceDefinition, bool allRolesIncluded)
		{
			return allRolesIncluded ? serviceDefinition.Roles : new[] { serviceDefinition.CurrentRole };
		}

		protected virtual ServiceDefinition Merge(ServiceDefinition serviceDefinition)
		{
			var roles = serviceDefinition.Roles ?? new Role[] { };
			var currentRole = roles.FirstOrDefault(role => role.IsCurrent);
			var currentRoleInstance = serviceDefinition.CurrentRoleInstance
				?? (currentRole != null ? currentRole.Instances.FirstOrDefault(instance => instance.IsCurrent) : null);

			return new ServiceDefinition
			{
				Roles = roles,
				CurrentRole = currentRole,
				CurrentRoleInstance = currentRoleInstance,
			};
		}

		protected virtual async Task SendRequest(IEnumerable<Role> roles, IEnumerable<RoleSite> sites, IPAddress localAddress, string internalEndpointName, TMessage message)
		{
			// send out the remote invalidation messages

			// if the advanced settings are used do not resolve the full application path since it will be defined directly in the settings

			var virtualPath = Options.CallbackPath.ToString();
			var path = sites == null ? VirtualPathUtility.ToAbsolute(virtualPath) : virtualPath;

			// trim the leading slash (treat as a relative path) so that the parent path is preserved

			path = path.TrimStart('/');

			var client = new HttpClient();
			var endpoints = GetRemoteEndpoints(roles, sites, localAddress, internalEndpointName).ToArray();

			foreach (var endpoint in endpoints)
			{
				var uri = new Uri(endpoint, path);

                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("uri={0}", uri));

				try
				{
					var response = await client.PostAsync(uri, message, new JsonMediaTypeFormatter()).WithCurrentCulture();

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("StatusCode={0}, uri={1}", response.StatusCode, uri));
                }
				catch (Exception e)
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("uri={0}", uri));
                    ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
                }
			}
		}

		protected virtual IEnumerable<Uri> GetRemoteEndpoints(IEnumerable<Role> roles, IEnumerable<RoleSite> sites, IPAddress localAddress, string internalEndpointName)
		{
			// find all endpoints with the specified internal endpoint name

			var endpoints =
				from role in roles
				from instance in role.Instances
				where sites != null || !IsCurrentRoleInstance(internalEndpointName, instance, localAddress) // ignore the current instance for basic configuration
				from epPair in instance.InstanceEndpoints
				from path in GetApplicationPaths(epPair.Key, internalEndpointName, sites)
				let ep = epPair.Value
				orderby role.Name, epPair.Key
				select new { RoleName = role.Name, EndpointName = epPair.Key, ep.Protocol, ep.IPEndPoint, Path = path };

			return endpoints.Select(endpoint => new Uri("{0}://{1}{2}/".FormatWith(endpoint.Protocol, endpoint.IPEndPoint, endpoint.Path)));
		}

		protected virtual bool IsCurrentRoleInstance(string internalEndpointName, RoleInstance instance, IPAddress localAddress)
		{
			return instance.IsCurrent
				|| Equals(instance.InstanceEndpoints[internalEndpointName].IPEndPoint.Address, localAddress);
		}

		protected virtual IEnumerable<string> GetApplicationPaths(string endpointName, string internalEndpointName, IEnumerable<RoleSite> sites)
		{
			// return the root application path based on the basic configuration

			if (string.Equals(endpointName, internalEndpointName)) yield return string.Empty;

			// return paths based on the advanced configuration

			var applications =
				from site in sites ?? new RoleSite[] { }
				from binding in site.Bindings ?? new Binding[] { }
				where binding.EndpointName == endpointName
				from application in site.VirtualApplications ?? new[] { new VirtualApplication() }
				select string.IsNullOrWhiteSpace(application.Name) ? string.Empty : "/" + application.Name;

			foreach (var application in applications)
			{
				// return the virtual application path

				yield return application;
			}
		}
	}
}
