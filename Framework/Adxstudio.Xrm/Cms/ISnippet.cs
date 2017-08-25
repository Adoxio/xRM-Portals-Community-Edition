/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface ISnippet
	{
		Entity Entity { get; }

		string Name { get; }

		/// <summary>
		/// Gets the DisplayName attribute of the entity record
		/// </summary>
		string DisplayName { get; }

		IPortalViewAttribute Value { get; }

		string LanguageName { get; }
	}
}
