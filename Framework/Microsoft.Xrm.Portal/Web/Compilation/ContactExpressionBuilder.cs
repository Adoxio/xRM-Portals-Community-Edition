/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;

namespace Microsoft.Xrm.Portal.Web.Compilation
{
	/// <summary>
	/// Expression builder for retrieving the current user for the current portal.
	/// </summary>
	/// <remarks>
	/// This class is superseded by the <see cref="PortalContextExpressionBuilder"/> class.
	/// <example>
	/// Configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <compilation>
	///    <expressionBuilders>
	///     <add expressionPrefix="Contact" type="Microsoft.Xrm.Portal.Web.Compilation.CurrentContactExpressionBuilder, Microsoft.Xrm.Portal"/>
	///    </expressionBuilders>
	///   </compilation>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ Contact: (Current|Attribute={attribute name}) [, Eval={expression}] [, Format={format string}] [, Default={default text}] [, Portal={portal name}] %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	/// <seealso cref="PortalContextExpressionBuilder"/>
	public sealed class CurrentContactExpressionBuilder : CrmExpressionBuilder<CurrentContactExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				var attributeName = arguments.GetValueByIndexOrName(0, "Attribute");
				var eval = arguments.GetValueByIndexOrName(1, "Eval");

				if (string.IsNullOrWhiteSpace(attributeName) && string.IsNullOrWhiteSpace(eval))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "(Current|Attribute={attribute name}) [, Eval={expression}] [, Format={format string}] [, Default={default text}] [, Portal={portal name}]");
				}

				var format = arguments.GetValueByIndexOrName(2, "Format");
				var defaultValue = arguments.GetValueByIndexOrName(3, "Default");
				var portalName = arguments.GetValueByIndexOrName(4, "Portal");
				var returnType = GetReturnType(controlType, propertyName);

				return GetEvalData("User", attributeName, eval, format, defaultValue, portalName, returnType);
			}
		}
	}
}
