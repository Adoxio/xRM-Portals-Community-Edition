/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web
{
	class SyncTracking
	{
		/// <summary>
		/// Method to process the node of the current request and save tracking info.
		/// </summary>
		public static void LogRequest(string ipAddress, Entity user, string originalPath, string portalName)
		{
			var context = PortalCrmConfigurationManager.CreatePortalContext(portalName);

			var notFoundLog = new Entity("adx_requestlog");

			notFoundLog.SetAttributeValue("adx_name", string.Format("Log of failed attempt to access {0} by IP address; {1}", originalPath, ipAddress));

			notFoundLog.SetAttributeValue("adx_date", DateTime.UtcNow);

			notFoundLog.SetAttributeValue("adx_ipaddress", ipAddress);

			//notFoundLog.SetAttributeValue("adx_originalpath", originalPath);

			var requestUrlEntity =
				context.ServiceContext.CreateQuery("adx_problemurlrequest").Where(
					pur => pur.GetAttributeValue<string>("adx_name") == originalPath).FirstOrDefault();

			if (requestUrlEntity == null)
			{
				var newRequestUrlEntity = new Entity("adx_problemurlrequest");

				newRequestUrlEntity.SetAttributeValue("adx_name", originalPath);

				newRequestUrlEntity.SetAttributeValue("adx_count", 1);

				context.ServiceContext.AddObject(newRequestUrlEntity);

				context.ServiceContext.SaveChanges();

				requestUrlEntity =
				context.ServiceContext.CreateQuery("adx_problemurlrequest").Where(
					pur => pur.GetAttributeValue<string>("adx_name") == originalPath).FirstOrDefault();
			}

			notFoundLog.SetRelatedEntity("adx_problemurlrequest_requestlog", requestUrlEntity);
			
			context.ServiceContext.AddObject(notFoundLog);

			context.ServiceContext.SaveChanges();
		}
	}
}
