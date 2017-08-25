/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;

namespace Microsoft.Xrm.Portal.Web.Compilation
{
	/// <summary>
	/// Expression builder for retrieving <see cref="IPortalContext"/> content (website, webpage, and user) for the current portal.
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
	///     <add expressionPrefix="Context" type="Microsoft.Xrm.Portal.Web.Compilation.PortalContextExpressionBuilder, Microsoft.Xrm.Portal"/>
	///    </expressionBuilders>
	///   </compilation>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ Context: (Current|Property={Website|User|Entity}) [, Attribute={attribute name}] [, Eval={expression}] [, Format={format string}] [, Default={default text}] [, Portal={portal name}] %>'/>
	/// <asp:Label runat="server" Text='<%$ Context: Property=User, Attribute=firstname, Format=-{0}-, Default=-unknown- %>'/>
	/// <asp:Label runat="server" Text='<%$ Context: Property=User, Eval=Firstname, Format=-{0}-, Default=-unknown- %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public sealed class PortalContextExpressionBuilder : CrmExpressionBuilder<PortalContextExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				if (string.IsNullOrEmpty(arguments.GetValueByIndexOrName(0, "Property")))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "(Current|Property={Website|User|Entity}) [, Attribute={attribute name}] [, Eval={expression}] [, Format={format string}] [, Default={default text}] [, Portal={portal name}]");
				}

				var property = arguments.GetValueByIndexOrName(0, "Property");
				var attributeName = arguments.GetValueByIndexOrName(1, "Attribute");
				var eval = arguments.GetValueByIndexOrName(2, "Eval");
				var format = arguments.GetValueByIndexOrName(3, "Format");
				var defaultValue = arguments.GetValueByIndexOrName(4, "Default");
				var portalName = arguments.GetValueByIndexOrName(5, "Portal");
				var returnType = GetReturnType(controlType, propertyName);

				return GetEvalData(property, attributeName, eval, format, defaultValue, portalName, returnType);
			}
		}
	}
}
