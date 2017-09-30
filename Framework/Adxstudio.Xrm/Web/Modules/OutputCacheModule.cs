/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Caching;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Threading;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Web.Modules
{
	/// <summary>
	/// Manages cache item dependencies between ASP.Net output cache and the <see cref="N:Adxstudio.Xrm.Caching"/> API.
	/// </summary>
	/// <remarks>
	/// This module propagates cache dependencies from the data cache to the output cache to allow changes in the data cache to invalidate the output cache.
	/// 
	/// It is enabled with the following configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///  </configSections>
	/// 
	///  <system.webServer>
	///   <modules runAllManagedModulesForAllRequests="true">
	///    <add name="OutputCacheModule" type="Adxstudio.Xrm.Web.Modules.OutputCacheModule, Adxstudio.Xrm" preCondition="managedHandler"/>
	///   </modules>
	///  </system.webServer>
	///  
	///  <microsoft.xrm.client>
	///   <objectCache default="Xrm">
	///    <add name="Xrm" type="Adxstudio.Xrm.Caching.OutputObjectCache, Adxstudio.Xrm"/>
	///   </objectCache>
	///  </microsoft.xrm.client>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// 
	/// Enable output caching on the ASP.Net page.
	/// <code>
	/// <![CDATA[
	/// <%@ Page ... %>
	/// <%@ OutputCache VaryByParam="*" Duration="60" %>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="OutputObjectCache"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class OutputCacheModule : IHttpModule
	{
		public void Dispose() { }

		public virtual void Init(HttpApplication application)
		{
			application.PostRequestHandlerExecute += LoadOutputCache;
		}

		private static void LoadOutputCache(object sender, EventArgs e)
		{
			var name = AdxstudioCrmConfigurationManager.GetCrmSection().OutputObjectCacheName;
			var keys = HttpSingleton<OutputCacheKeyCollection>.GetInstance(name, () => new OutputCacheKeyCollection());

			HttpContext.Current.Response.AddCacheItemDependencies(keys.ToArray());
		}
	}
}
