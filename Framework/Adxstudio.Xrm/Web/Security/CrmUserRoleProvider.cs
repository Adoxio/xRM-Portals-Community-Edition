/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Security
{
	/// <summary>
	/// A <see cref="CrmRoleProvider"/> that validates 'systemuser' entities (users) against 'adx_webrole' entitles (roles).
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <roleManager enabled="true" defaultProvider="Xrm">
	///    <providers>
	///     <add
	///      name="Xrm"
	///      type="Microsoft.Xrm.Portal.Web.Security.CrmUserRoleProvider"
	///      portalName="Xrm" [Microsoft.Xrm.Portal.Configuration.PortalContextElement]
	///      attributeMapIsAuthenticatedUsersRole="adx_authenticatedusersrole"
	///      attributeMapRoleName="adx_name"
	///      attributeMapRoleWebsiteId="adx_websiteid"
	///      attributeMapUsername="domainname"
	///      roleEntityName="adx_webrole"
	///      roleToUserRelationshipName="adx_webrole_systemuser"
	///      userEntityName="systemuser"
	///     />
	///    </providers>
	///   </roleManager>
	///  </system.web>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class CrmUserRoleProvider : CrmRoleProvider
	{
		public override void Initialize(string name, NameValueCollection config)
		{
			config["attributeMapIsDisabled"] = config["attributeMapIsDisabled"] ?? "isdisabled";

			config["attributeMapStateCode"] = config["attributeMapStateCode"] ?? "statecode";

			config["attributeMapIsAuthenticatedUsersRole"] = config["attributeMapIsAuthenticatedUsersRole"] ?? "adx_authenticatedusersrole";

			config["attributeMapRoleName"] = config["attributeMapRoleName"] ?? "adx_name";

			config["attributeMapRoleWebsiteId"] = config["attributeMapRoleWebsiteId"] ?? "adx_websiteid";

			config["attributeMapUsername"] = config["attributeMapUsername"] ?? "domainname";

			config["roleEntityName"] = config["roleEntityName"] ?? "adx_webrole";

			config["roleToUserRelationshipName"] = config["roleToUserRelationshipName"] ?? "adx_webrole_systemuser";

			config["userEntityName"] = config["userEntityName"] ?? "systemuser";

			base.Initialize(name, config);
		}
	}
}
