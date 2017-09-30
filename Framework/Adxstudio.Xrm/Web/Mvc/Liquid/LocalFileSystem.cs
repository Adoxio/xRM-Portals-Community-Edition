/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Xrm.Client;
using Newtonsoft.Json.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class LocalFileSystem : IComposableFileSystem
	{
		public LocalFileSystem(string root)
		{
			if (root == null) throw new ArgumentNullException("root");

			Root = root;
		}

		public string Root { get; private set; }

		public IEnumerable<TemplateFileInfo> GetTemplateFiles()
		{
			var root = new DirectoryInfo(Root);

			if (!root.Exists)
			{
				return Enumerable.Empty<TemplateFileInfo>();
			}

			var rootUri = new Uri(
				root.FullName.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture))
					? root.FullName
					: root.FullName + Path.DirectorySeparatorChar);

			return root.EnumerateFiles("*", SearchOption.AllDirectories)
				.Where(file => string.Equals(Path.GetExtension(file.Name), ".liquid", StringComparison.InvariantCulture) || string.Equals(Path.GetExtension(file.Name), ".json", StringComparison.InvariantCulture))
				.GroupBy(file => GetTemplateFileName(rootUri, file), e => e, StringComparer.InvariantCulture)
				.Where(e => Regex.IsMatch(e.Key, @"^[a-zA-Z0-9_\/]+$"))
				.Select(e => new TemplateFileInfo(e.Key, GetTemplateMetadata(e)));
		}

		public string ReadTemplateFile(Context context, string templateName)
		{
			string template;

			if (TryReadTemplateFile(context, templateName, out template))
			{
				return template;
			}

			throw new FileSystemException("Template {0} not found.", context[templateName] as string);
		}

		public bool TryReadTemplateFile(Context context, string templateName, out string template)
		{
			var templatePath = (string)context[templateName];

			return TryReadTemplateFile(templatePath, out template);
		}

		public bool TryReadTemplateFile(string templateName, out string template)
		{
			template = null;

			string fullPath;

			if (!TryGetFullPath(templateName, out fullPath))
			{
				return false;
			}

			if (!File.Exists(fullPath))
			{
				return false;
			}

			template = File.ReadAllText(fullPath);

			return true;
		}

		private JObject GetTemplateMetadata(IEnumerable<FileInfo> files)
		{
			var metadataFile = files
				.FirstOrDefault(e => string.Equals(Path.GetExtension(e.Name), ".json", StringComparison.InvariantCulture));

			return metadataFile == null
				? null
				: GetTemplateMetadata(metadataFile);
		}

		private JObject GetTemplateMetadata(FileInfo file)
		{
			if (!file.Exists)
			{
				return null;
			}

			try
			{
				return JObject.Parse(File.ReadAllText(file.FullName));
			}
			catch
			{
				return null;
			}
		}

		private bool TryGetFullPath(string templatePath, out string fullPath)
		{
			fullPath = null;

			if (templatePath == null || !Regex.IsMatch(templatePath, @"^[a-zA-Z0-9_][a-zA-Z0-9_\/]*$"))
			{
				return false;
			}

			try
			{
				fullPath = templatePath.Contains("/")
					? Path.Combine(Path.Combine(Root, Path.GetDirectoryName(templatePath)), "{0}.liquid".FormatWith(Path.GetFileName(templatePath)))
					: Path.Combine(Root, "{0}.liquid".FormatWith(templatePath));

				var escapedPath = Root.Replace(@"\", @"\\").Replace("(", @"\(").Replace(")", @"\)");

				return Regex.IsMatch(Path.GetFullPath(fullPath), "^{0}".FormatWith(escapedPath));
			}
			catch (System.ArgumentException)
			{
				return false;
			}
		}

		private static string GetTemplateFileName(Uri rootUri, FileInfo file)
		{
			var fileUri = new Uri(Path.Combine(file.DirectoryName, Path.GetFileNameWithoutExtension(file.Name)));

			return rootUri.MakeRelativeUri(fileUri).ToString();
		}
	}
}
