/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web.Mvc.Html;
using DotLiquid;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class Filters
	{
		public static object Default(object input, object @default)
		{
			return input ?? @default;
		}

		public static string FileSize(object value, int precision = 1)
		{
			return value == null
				? null
				: new FileSize(Convert.ToUInt64(value)).ToString(precision);
		}

		public static bool HasRole(UserDrop user, string roleName)
		{
			return user != null && user.IsUserInRole(roleName);
		}

		public static object Liquid(Context context, object input)
		{
			if (input == null)
			{
				return null;
			}

			var html = context.Registers["htmlHelper"] as HtmlHelper;

			if (html == null)
			{
				return input;
			}

			string result = null;

			context.Stack(() =>
			{
				result = html.Liquid(input.ToString(), context);
			});

			return result;
		}

		public static object WebTemplate(Context context, object input)
		{
			EntityReference webTemplateReference;

			if (!TryGetWebTemplateReference(input, out webTemplateReference))
			{
				return null;
			}

			var html = context.Registers["htmlHelper"] as HtmlHelper;

			if (html == null)
			{
				return null;
			}

			string result = null;

			context.Stack(() =>
			{
				result = html.WebTemplate(webTemplateReference, context);
			});

			return result;
		}

		private static bool TryGetWebTemplateReference(object input, out EntityReference webTemplateReference)
		{
			webTemplateReference = null;

			if (input == null)
			{
				return false;
			}

			var referenceDrop = input as EntityReferenceDrop;

			if (referenceDrop != null)
			{
				webTemplateReference = referenceDrop.ToEntityReference();

				return true;
			}

			Guid id;

			if (Guid.TryParse(input.ToString(), out id))
			{
				webTemplateReference = new EntityReference("adx_webtemplate", id);

				return true;
			}

			return false;
		}
	}
}
