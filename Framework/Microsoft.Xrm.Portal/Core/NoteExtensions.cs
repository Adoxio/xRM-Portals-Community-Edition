/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Core
{
	public static class NoteExtensions
	{
		/// <summary>
		/// Retrieve the URL to emit that will download this attached file from the website.
		/// </summary>
		public static string GetRewriteUrl(this Entity note)
		{
			note.AssertEntityName("annotation");

			return VirtualPathUtility.ToAbsolute("~/_entity/{0}/{1}".FormatWith(note.LogicalName, note.Id));
		}
	}
}
