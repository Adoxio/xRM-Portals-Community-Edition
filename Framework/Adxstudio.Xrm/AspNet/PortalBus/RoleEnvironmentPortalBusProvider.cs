/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.WindowsAzure.ServiceRuntime.Configuration;
using Owin;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// A portal bus that uses Azure cloud service internal endpoints for sending messages to remote instances.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public class RoleEnvironmentPortalBusProvider<TMessage> : ServiceDefinitionPortalBusProvider<TMessage>
	{
		public RoleEnvironmentPortalBusProvider(IAppBuilder app, ServiceDefinitionPortalBusOptions<TMessage> options)
			: base(app, options)
		{
		}

		protected override ServiceDefinition Merge(ServiceDefinition serviceDefinition)
		{
			var roles = serviceDefinition.Roles ?? new Role[] { };
			var joined = roles.Select(role => ToRole(role, ToRole(role.Name)));
			var merged = joined.Union(Except(serviceDefinition)).ToArray();
			var currentRole = merged.FirstOrDefault(role => role.IsCurrent);
			var currentRoleInstance = serviceDefinition.CurrentRoleInstance
				?? (currentRole != null ? currentRole.Instances.FirstOrDefault(instance => instance.IsCurrent) : null);

			return new ServiceDefinition
			{
				Roles = merged,
				CurrentRole = currentRole,
				CurrentRoleInstance = currentRoleInstance,
			};
		}

		private static IEnumerable<Role> Except(ServiceDefinition serviceDefinition)
		{
			if (!Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.IsAvailable)
			{
				return new Role[] { };
			}

			return Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.Roles.Values
				.Where(role => serviceDefinition.Roles == null || serviceDefinition.Roles.All(r => !string.Equals(r.Name, role.Name, StringComparison.OrdinalIgnoreCase)))
				.Select(ToRole);
		}

		private static Microsoft.WindowsAzure.ServiceRuntime.Role ToRole(string name)
		{
			if (string.IsNullOrWhiteSpace(name) || !Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.IsAvailable)
			{
				return null;
			}

			Microsoft.WindowsAzure.ServiceRuntime.Role role;
			return Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.Roles.TryGetValue(name, out role) ? role : null;
		}

		private static Role ToRole(Role role1, Microsoft.WindowsAzure.ServiceRuntime.Role role2)
		{
			if (role1 == null && role2 == null) return null;
			if (role2 == null) return role1;
			if (role1 == null) return ToRole(role2);

			return new Role
			{
				Name = role1.Name ?? role2.Name,
				IsCurrent = role1.IsCurrent || role2 == Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.CurrentRoleInstance.Role,
				Sites = role1.Sites,
				Instances = role1.Instances ?? (role2.Instances != null ? role2.Instances.Select(ToRoleInstance).ToList() : null),
			};
		}

		private static Role ToRole(Microsoft.WindowsAzure.ServiceRuntime.Role role)
		{
			return new Role
			{
				Name = role.Name,
				IsCurrent = role == Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.CurrentRoleInstance.Role,
				Instances = role.Instances.Select(ToRoleInstance).ToList(),
			};
		}

		private static RoleInstance ToRoleInstance(Microsoft.WindowsAzure.ServiceRuntime.RoleInstance instance)
		{
			return new RoleInstance
			{
				IsCurrent = instance == Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.CurrentRoleInstance,
				InstanceEndpoints = instance.InstanceEndpoints.Select(pair => new { pair.Key, Value = ToRoleInstanceEndpoint(pair.Value) }).ToDictionary(pair => pair.Key, pair => pair.Value)
			};
		}

		private static RoleInstanceEndpoint ToRoleInstanceEndpoint(Microsoft.WindowsAzure.ServiceRuntime.RoleInstanceEndpoint endpoint)
		{
			return new RoleInstanceEndpoint
			{
				Protocol = endpoint.Protocol,
				IPEndPoint = endpoint.IPEndpoint,
			};
		}
	}
}
