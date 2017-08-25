/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Net;

namespace Adxstudio.Xrm.WindowsAzure.ServiceRuntime.Configuration
{
	/// <summary>
	/// Represents the configuration of an Azure cloud service.
	/// </summary>
	public class ServiceDefinition
	{
		public ICollection<Role> Roles { get; set; }
		public Role CurrentRole { get; set; }
		public RoleInstance CurrentRoleInstance { get; set; }
	}

	/// <summary>
	/// Represents the configuration of an Azure role.
	/// </summary>
	public class Role
	{
		public string Name { get; set; }
		public bool IsCurrent { get; set; }
		public ICollection<RoleSite> Sites { get; set; }
		public ICollection<RoleInstance> Instances { get; set; }
	}

	/// <summary>
	/// Represents the configuration of an Azure site.
	/// </summary>
	public class RoleSite
	{
		public string Name { get; set; }
		public ICollection<VirtualApplication> VirtualApplications { get; set; }
		public ICollection<Binding> Bindings { get; set; }
	}

	/// <summary>
	/// Represents the configuration of an Azure virtual application.
	/// </summary>
	public class VirtualApplication
	{
		public string Name { get; set; }
	}

	/// <summary>
	/// Represents the configuration of an Azure binding.
	/// </summary>
	public class Binding
	{
		public string EndpointName { get; set; }
	}

	/// <summary>
	/// Represents the configuration of an Azure role instance.
	/// </summary>
	public class RoleInstance
	{
		public bool IsCurrent { get; set; }
		public IDictionary<string, RoleInstanceEndpoint> InstanceEndpoints { get; set; }
	}

	/// <summary>
	/// Represents the configuration of an Azure role instance endpoint.
	/// </summary>
	public class RoleInstanceEndpoint
	{
		public string Protocol { get; set; }
		public IPEndPoint IPEndPoint { get; set; }
	}
}
