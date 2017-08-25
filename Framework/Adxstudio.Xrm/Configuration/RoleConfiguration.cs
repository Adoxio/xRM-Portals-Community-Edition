/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// Represents the configuration of an Azure role.
	/// </summary>
	[Serializable]
	[DataContract]
	public class RoleConfiguration
	{
		[XmlElement]
		[DataMember]
		public ICollection<RoleSite> Sites;
	}

	/// <summary>
	/// Represents the configuration of an Azure site.
	/// </summary>
	[Serializable]
	[DataContract]
	public class RoleSite
	{
		[XmlElement]
		[DataMember]
		public string Name;

		[XmlElement]
		[DataMember]
		public ICollection<RoleVirtualApplication> VirtualApplications;

		[XmlElement]
		[DataMember]
		public ICollection<RoleBinding> Bindings;
	}

	/// <summary>
	/// Represents the configuration of an Azure virtual application.
	/// </summary>
	[Serializable]
	[DataContract]
	public class RoleVirtualApplication
	{
		[XmlElement]
		[DataMember]
		public string Name;
	}

	/// <summary>
	/// Represents the configuration of an Azure binding.
	/// </summary>
	[Serializable]
	[DataContract]
	public class RoleBinding
	{
		[XmlElement]
		[DataMember]
		public string EndpointName;
	}
}
