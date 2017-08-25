/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Compilation
{
	using System;
	using System.Collections.Specialized;
	using System.Web;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Resources;
	using Microsoft.Xrm.Portal.Web.Compilation;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// New custom expression builder for Snippets
	/// </summary>
	public sealed class SnippetExpressionBuilder : CrmExpressionBuilder<SnippetExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				if (string.IsNullOrEmpty(NameValueCollectionExtensions.GetValueByIndexOrName(arguments, 0, "Name")))
				{
					CrmExpressionBuilder<SnippetExpressionBuilder.Provider>.ThrowArgumentException(propertyName, expressionPrefix, "Name={snippet name} [, Key={ResourceManager key}] [, Format={format string}] [, Portal={portal name}]");
				}

				string valueByIndexOrName1 = NameValueCollectionExtensions.GetValueByIndexOrName(arguments, 0, "Name");
				string key = NameValueCollectionExtensions.GetValueByIndexOrName(arguments, 1, "Key") ?? string.Empty;
				string valueByIndexOrName2 = NameValueCollectionExtensions.GetValueByIndexOrName(arguments, 2, "Format");
				Type returnType = CrmExpressionBuilder<SnippetExpressionBuilder.Provider>.GetReturnType(controlType, propertyName);

				var adapter = new SnippetDataAdapter(new PortalConfigurationDataAdapterDependencies());
				var snippet = adapter.Select(valueByIndexOrName1);

				var snippetByName = snippet == null ? null : snippet.Entity;

				if (Microsoft.Xrm.Client.TypeExtensions.IsA(returnType, typeof(Entity)))
				{
					return (object)snippetByName;
				}

				if (snippetByName == null)
				{
					string value = ResourceManager.GetString(key);
					if (value == null)
					{
						return (object)key;
					}

					return (object)value;
				}

				return CrmExpressionBuilder<SnippetExpressionBuilder.Provider>.GetEvalData((object)snippetByName, "adx_value", (string)null, valueByIndexOrName2, returnType);
			}
		}
	}
}

