/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Defines the xml namespaces for SDK contracts.
	/// </summary>
	internal static class V5
	{
		//define the current prefix for the namespace in one location
		public const string Prefix = "http://schemas.microsoft.com/xrm/2011/";

		/// <summary>
		/// Defines the namespace for common data contracts.
		/// </summary>
		public const string Contracts = Prefix + "Contracts";

		/// <summary>
		/// Defines namespace for Metadata contracts.
		/// </summary>
		public const string Metadata = Prefix + "Metadata";

		/// <summary>
		/// Defines namespace for workflow interfaces and contracts.
		/// </summary>
		public const string Workflow = Prefix + "Workflow";

		public const string Discovery = Contracts + "/Discovery";

		/// <summary>
		/// Defines namespace for services
		/// </summary>
		public const string Services = Contracts + "/Services";

		/// <summary>
		/// Defines namespace for claims.
		/// </summary>
		public const string Claims = Prefix + "Claims";

		/// <summary>
		/// Organization SDK endpoint
		/// </summary>
		public const string OrganizationEndpoint = "2011/Organization.svc";
	}
}
