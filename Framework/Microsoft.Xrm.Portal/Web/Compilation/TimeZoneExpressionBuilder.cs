/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using Microsoft.Xrm.Portal.Configuration;

namespace Microsoft.Xrm.Portal.Web.Compilation
{
	/// <summary>
	/// Expression builder for retrieving the timezone for the current portal.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <compilation>
	///    <expressionBuilders>
	///     <add expressionPrefix="TimeZone" type="Microsoft.Xrm.Portal.Web.Compilation.TimeZoneExpressionBuilder, Microsoft.Xrm.Portal"/>
	///    </expressionBuilders>
	///   </compilation>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ TimeZone: [Name=(id|displayname)] [, Eval={expression}] [, Format={format string}] [, Portal={portal name}] %>'/>
	/// <asp:Label runat="server" Text='<%$ TimeZone: Eval=DisplayName, Format=-{0}- %>'/>
	/// <asp:Label runat="server" Text='<%$ TimeZone: Eval=Id, Format=-{0}- %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public sealed class TimeZoneExpressionBuilder : CrmExpressionBuilder<TimeZoneExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				var name = arguments.GetValueByIndexOrName(0, "Name");
				var eval = arguments.GetValueByIndexOrName(1, "Eval");

				if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(eval))
				{
					ThrowException(propertyName, expressionPrefix);
				}

				var format = arguments.GetValueByIndexOrName(2, "Format");
				var portalName = arguments.GetValueByIndexOrName(3, "Portal");
				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
				var timeZone = portal.GetTimeZone();

				if (!string.IsNullOrWhiteSpace(name))
				{
					if (string.Compare(name, "id", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						return timeZone.Id;
					}

					if (string.Compare(name, "displayname", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						return timeZone.DisplayName;
					}

					ThrowException(propertyName, expressionPrefix);
				}

				return timeZone != null
					? GetEvalData(timeZone, null, eval, format)
					: null;
			}

			private static void ThrowException(string propertyName, string expressionPrefix)
			{
				ThrowArgumentException(propertyName, expressionPrefix, "[Name=(id|displayname)] [, Eval={expression}] [, Format={format string}] [, Portal={portal name}]");
			}
		}
	}
}
