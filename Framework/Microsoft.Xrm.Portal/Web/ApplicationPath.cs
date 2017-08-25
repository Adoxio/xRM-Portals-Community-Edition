/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Represents the various forms of virtual path values or an alternative external URL.
	/// </summary>
	public class ApplicationPath // MSBug #120128: Won't seal, is returned by a number of key APIs, inheritance is expected extension point.
	{
		/// <summary>
		/// Gets the app-relative path with the leading '~' character.
		/// </summary>
		public string AppRelativePath { get; private set; }


		/// <summary>
		/// Gets the app-relative path without the leading '~' character.
		/// </summary>
		public string PartialPath { get; private set; }

		/// <summary>
		/// Gets the absolute application path.
		/// </summary>
		public string AbsolutePath { get; private set; }

		/// <summary>
		/// Gets the external URL.
		/// </summary>
		public string ExternalUrl { get; private set; }

		/// <summary>
		/// Builds an <see cref="ApplicationPath"/> from an app-relative path.
		/// </summary>
		public static ApplicationPath FromAppRelativePath(string appRelativePath)
		{
			if (!VirtualPathUtility.IsAppRelative(appRelativePath))
			{
				throw new ArgumentException("The path '{0}' is not a valid app-relative path.".FormatWith(appRelativePath), "appRelativePath");
			}

			var absolutePath = VirtualPathUtility.ToAbsolute(appRelativePath);
			var partialPath = appRelativePath.TrimStart('~');

			return new ApplicationPath { AppRelativePath = appRelativePath, PartialPath = partialPath, AbsolutePath = absolutePath };
		}

		/// <summary>
		/// Builds an <see cref="ApplicationPath"/> from a partial app-relative path.
		/// </summary>
		public static ApplicationPath FromPartialPath(string partialPath)
		{
			return FromAppRelativePath("~" + partialPath);
		}

		/// <summary>
		/// Builds an <see cref="ApplicationPath"/> from an absolute path.
		/// </summary>
		public static ApplicationPath FromAbsolutePath(string absolutePath)
		{
			if (!VirtualPathUtility.IsAbsolute(absolutePath))
			{
				throw new ArgumentException("The path '{0}' is not a valid absolute path.".FormatWith(absolutePath), "absolutePath");
			}

			var appRelativePath = VirtualPathUtility.ToAppRelative(absolutePath);
			var partialPath = appRelativePath.TrimStart('~');

			return new ApplicationPath { AppRelativePath = appRelativePath, PartialPath = partialPath, AbsolutePath = absolutePath };
		}

		/// <summary>
		/// Builds an <see cref="ApplicationPath"/> from an external URL.
		/// </summary>
		public static ApplicationPath FromExternalUrl(string externalUrl)
		{
			externalUrl.ThrowOnNullOrWhitespace("externalUrl");

			return new ApplicationPath { ExternalUrl = externalUrl };
		}

		public static ApplicationPath Parse(string rawUrl)
		{
			// try parse the URL as an absolute path

			var appRelativePath = VirtualPathUtility.ToAppRelative(rawUrl);

			// the URL either becomes a valid app-relative path or remains a partial path

			return VirtualPathUtility.IsAppRelative(appRelativePath)
				? FromAppRelativePath(appRelativePath)
				: FromPartialPath(appRelativePath);
		}
	}
}
